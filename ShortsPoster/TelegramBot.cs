using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using ShortsPoster.Db;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using File = System.IO.File;
using ShortsPoster.Interfaces;
using ShortsPoster.Util;

namespace ShortsPoster;

public sealed class TelegramBotService : BackgroundService
{
    private readonly ITelegramBotClient _bot;
    private readonly IServiceProvider _services;
    private readonly IConfiguration _cfg;
    private readonly ILogger<TelegramBotService> _log;
    private readonly IVideoPoster _videoPoster;

    private enum Step { None, WaitVideo, WaitTitle, WaitDesc, WaitTags }

    private sealed record Session(
        Step State,
        string VideoPath,
        string? Title,
        string? Description
    );

    private readonly ConcurrentDictionary<long, Session> _sessions = new();

    public TelegramBotService(IServiceProvider services,
                              IConfiguration cfg,
                              ILogger<TelegramBotService> log)
    {
        _services = services;
        _cfg = cfg;
        _log = log;

        var token = _cfg["TelegramBotToken"]
                    ?? throw new InvalidOperationException("TelegramBotToken env missing");
       
        _bot = new TelegramBotClient(token);
        _videoPoster = new YouTubePoster(cfg, services);

    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _bot.StartReceiving(HandleUpdateAsync,
                            HandleErrorAsync,
                            new() { AllowedUpdates = Array.Empty<UpdateType>() },
                            stoppingToken);

        var me = await _bot.GetMe(stoppingToken);
        _log.LogInformation("Bot @{u} started", me.Username);
    }

    private async Task HandleUpdateAsync(ITelegramBotClient bot, Update upd, CancellationToken ct)
    {
        if (upd.Message is not { } msg) return;
        var chatId = msg.Chat.Id;
        var uid = msg.From!.Id;

        if (msg.Text is { } txt && txt.StartsWith('/'))
        {
            switch (txt.Split(' ')[0])
            {
                case "/start":
                    await bot.SendMessage(chatId,
                        "Привет! /authorize — привязать YouTube, /upload — загрузить шортс.");
                    return;

                case "/authorize":
                    await bot.SendMessage(chatId,
                        $"Авторизуйтесь по ссылке:\n{GenerateAuthUrl(uid)}");
                    return;

                case "/upload":
                    _sessions[uid] = new Session(Step.WaitVideo, "", null, null);
                    await bot.SendMessage(chatId, "Пришлите вертикальное видео ≤60 с.");
                    return;
            }
        }

        _sessions.TryGetValue(uid, out var s);
        if (s == null) return;

        switch (s.State)
        {
            case Step.WaitVideo when msg.Video is { } vid:
                {
                    if (vid.Duration > 60 || vid.Width >= vid.Height)
                    {
                        await bot.SendMessage(chatId,
                            "Нужен вертикальный ролик ≤ 60 с. Попробуйте снова.");
                        return;
                    }

                    var file = await bot.GetFile(vid.FileId, ct);
                    var tmpFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.mp4");
                    await using var fs = File.OpenWrite(tmpFile);
                    await bot.DownloadFile(file.FilePath!, fs, ct);

                    _sessions[uid] = s with { State = Step.WaitTitle, VideoPath = tmpFile };
                    await bot.SendMessage(chatId, "Введите заголовок:");
                    break;
                }

            case Step.WaitTitle when msg.Text is { } t:
                _sessions[uid] = s with { State = Step.WaitDesc, Title = t.Trim() };
                await bot.SendMessage(chatId, "Описание:");
                break;

            case Step.WaitDesc when msg.Text is { } d:
                {
                    _sessions[uid] = s with { State = Step.WaitTags, Description = d.Trim() };

                    var kb = new ReplyKeyboardMarkup(
                                 new[] { new[] { new KeyboardButton("Пропустить теги") } })
                    { ResizeKeyboard = true, OneTimeKeyboard = true };

                    await bot.SendMessage(chatId,
                        "Теги через запятую (или нажмите «Пропустить теги»):", replyMarkup: kb);
                    break;
                }

            case Step.WaitTags:
                {
                    string tagLine = msg.Text ?? "";
                    await FinishUploadAsync(uid, chatId, tagLine, ct);
                    _sessions.TryRemove(uid, out _);
                    break;
                }
        }
    }

    private async Task FinishUploadAsync(long uid, long chatId, string tagLine, CancellationToken ct)
    {
        var sess = _sessions[uid];

        await using var scope = _services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        string[] tags;
        if (tagLine.Equals("Пропустить теги", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(tagLine))
        {
            tags = db.LastTags
                     .AsNoTracking()
                     .Where(x => x.TelegramUserId == uid)
                     .Select(x => x.TagsCsv)
                     .FirstOrDefault()?
                     .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                     ?? Array.Empty<string>();
        }
        else
        {
            tags = tagLine.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var row = await db.LastTags.FirstOrDefaultAsync(x => x.TelegramUserId == uid, ct);
            if (row == null)
                db.LastTags.Add(new() { TelegramUserId = uid, TagsCsv = string.Join(',', tags) });
            else
                row.TagsCsv = string.Join(',', tags);
            var old = db.LastTags.Where(x => x.TelegramUserId == uid);
            db.LastTags.RemoveRange(old);
            db.LastTags.Add(new() { TelegramUserId = uid, TagsCsv = string.Join(',', tags) });
            await db.SaveChangesAsync(ct);
        }

        var tok = await db.UserTokens.AsNoTracking()
                       .FirstOrDefaultAsync(x => x.TelegramUserId == uid, ct);
        if (tok == null)
        {
            await _bot.SendMessage(chatId, "Сначала /authorize");
            File.Delete(sess.VideoPath);
            return;
        }
        try
        {
            var url = await _videoPoster.UploadAsync(uid, sess.VideoPath, sess.Title!, sess.Description!, tags, ct);
            await _bot.SendMessage(chatId, $"✅ Видео загружено! {url}");
        }
        catch (Exception ex)
        {
            await _bot.SendMessage(chatId, $"Ошибка загрузки: {ex.Message}");
        }
        finally
        {
            File.Delete(sess.VideoPath);
        }
    }

    private Task HandleErrorAsync(ITelegramBotClient _, Exception ex, CancellationToken _2)
    {
        _log.LogError(ex, "Telegram polling error");
        return Task.CompletedTask;
    }

    private string GenerateAuthUrl(long tgUserId)
    {
        var cid = _cfg["GoogleOAuth:ClientId"];
        var red = _cfg["GoogleOAuth:RedirectUri"];
        var scope = "https://www.googleapis.com/auth/youtube.upload";

        return $"https://accounts.google.com/o/oauth2/v2/auth" +
               $"?scope={Uri.EscapeDataString(scope)}" +
               $"&access_type=offline" +
               $"&include_granted_scopes=true" +
               $"&response_type=code" +
               $"&state={tgUserId}" +
               $"&redirect_uri={Uri.EscapeDataString(red)}" +
               $"&client_id={cid}";
    }
}
