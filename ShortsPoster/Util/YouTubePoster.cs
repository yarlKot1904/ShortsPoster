using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.EntityFrameworkCore;
using ShortsPoster.Db;
using ShortsPoster.Interfaces;

namespace ShortsPoster.Util
{
    

    public class YouTubePoster : IVideoPoster
    {
        private readonly IConfiguration _cfg;
        private readonly IServiceProvider _services;
        private readonly ILogger<YouTubePoster> _log;

        public YouTubePoster(IConfiguration cfg, IServiceProvider services)
        {
            _cfg = cfg;
            _services = services;
            _log = services.GetRequiredService<ILogger<YouTubePoster>>();
        }

        public async Task<string> UploadAsync(long tgUserId, string videoPath, string title, string description, string[] tags, CancellationToken ct)
        {
            await using var scope = _services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var tok = await db.UserTokens.AsNoTracking()
                           .FirstOrDefaultAsync(x => x.TelegramUserId == tgUserId, ct);
            if (tok == null)
                throw new InvalidOperationException("Пользователь не авторизован");

            var cred = BuildUserCredential(tok.RefreshToken, tgUserId);
            _log.LogInformation("Building credential for {tgUserId}, {cred.UserId}", tgUserId,  cred.UserId);
            var yt = new YouTubeService(new BaseClientService.Initializer
            {
                HttpClientInitializer = cred,
                ApplicationName = "TelegramYouTubeBot"
            });

            var video = new Google.Apis.YouTube.v3.Data.Video
            {
                Snippet = new()
                {
                    Title = title,
                    Description = description + "\n#" + string.Join(" #", tags),
                    Tags = tags
                },
                Status = new()
                {
                    PrivacyStatus = "public",
                    MadeForKids = false,
                    SelfDeclaredMadeForKids = false
                }
            };

            string videoId;
            _log.LogInformation("Uploading video {videoPath}", videoPath);
            await using (var fs = File.OpenRead(videoPath))
            {
                var req = yt.Videos.Insert(video, "snippet,status", fs, "video/*");
                await req.UploadAsync(ct);
                _log.LogInformation("Video upload complete");
                _log.LogInformation("Response: {response}", req.ResponseBody);
                videoId = req.ResponseBody?.Id!;
            }

            return "https://youtu.be/" + videoId;
        }

        private UserCredential BuildUserCredential(string refreshToken, long tgUserId)
        {
            var flow = new GoogleAuthorizationCodeFlow(new()
            {
                ClientSecrets = new()
                {
                    ClientId = _cfg["GoogleOAuth:ClientId"],
                    ClientSecret = _cfg["GoogleOAuth:ClientSecret"]
                },
                Scopes = new[] { YouTubeService.Scope.YoutubeUpload }
            });

            return new UserCredential(flow, tgUserId.ToString(),
                                      new TokenResponse { RefreshToken = refreshToken });
        }
    }

}
