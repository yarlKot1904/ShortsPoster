using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShortsPoster.Db;

namespace ShortsPoster
{
    public class Program
    {
        public static ILogger logger;
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Services.AddControllers();
            
            builder.Services.AddDbContext<AppDbContext>(opts =>
                opts.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
            
            
            builder.Services.AddHostedService<TelegramBotService>();

            var app = builder.Build();
            logger = app.Logger;
            app.MapControllers();
            app.Run();

        }
    }
}
