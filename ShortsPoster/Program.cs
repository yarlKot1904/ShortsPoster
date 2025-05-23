using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShortsPoster.Db;
using Serilog;

namespace ShortsPoster
{
    public class Program
    {
        public static Microsoft.Extensions.Logging.ILogger logger;

        public static async Task Main(string[] args)
        {
            // Настройка Serilog
          
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug() // общий минимальный уровень (для всех)
                .WriteTo.Console(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information) // консоль — Info и выше
                .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug) // файл — Debug и выше
                .CreateLogger();


            try
            {
                Log.Information("Запуск приложения");

                var builder = WebApplication.CreateBuilder(args);

                // Используем Serilog как логгер
                builder.Host.UseSerilog();

                builder.Services.AddControllers();

                builder.Services.AddDbContext<AppDbContext>(opts =>
                    opts.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

                builder.Services.AddHostedService<TelegramBotService>();

                var app = builder.Build();

                logger = app.Logger;
                app.MapControllers();
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Ошибка при запуске приложения");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
