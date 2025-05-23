namespace ShortsPoster.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Google.Apis.Auth.OAuth2.Responses;
    using Google.Apis.Auth.OAuth2.Flows;
    using Google.Apis.Auth.OAuth2;
    using Google.Apis.Util.Store;
    using System.Threading.Tasks;
    using System.Linq;
    using ShortsPoster.Db;
    using ShortsPoster.Models;
    using Microsoft.EntityFrameworkCore;

    [ApiController]
    [Route("oauth")]
    public class OAuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly ILogger<OAuthController> _logger;

        public OAuthController(AppDbContext db, IConfiguration config, ILogger<OAuthController> logger)
        {
            _db = db;
            _config = config;
            _logger = logger;
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback(string code, string state)
        {
            // state — telegramUserId
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
            {
                _logger.LogWarning("Callback received with missing code or state");
                return BadRequest("Missing code or state");
            }
            var telegramUserId = long.Parse(state);

            _logger.LogInformation("Processing OAuth callback for Telegram user {UserId}", telegramUserId);

            var clientId = _config["GoogleOAuth:ClientId"];
            var clientSecret = _config["GoogleOAuth:ClientSecret"];
            var redirectUri = _config["GoogleOAuth:RedirectUri"];

            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                },
                Scopes = new[] { Google.Apis.YouTube.v3.YouTubeService.Scope.YoutubeUpload }
            });

            TokenResponse token = await flow.ExchangeCodeForTokenAsync(
                userId: telegramUserId.ToString(),
                code: code,
                redirectUri: redirectUri,
                taskCancellationToken: CancellationToken.None);
            Program.logger.LogInformation("token1 " + token.RefreshToken);
            Program.logger.LogInformation("token2 " + token.AccessToken);
            Program.logger.LogInformation("token3 " + token.TokenType);

            _logger.LogInformation("Token received for user {UserId}", telegramUserId);
            _logger.LogDebug("AccessToken: {AccessToken}, RefreshToken: {RefreshToken}, TokenType: {TokenType}",
                        token.AccessToken, token.RefreshToken, token.TokenType);

            var userToken = _db.UserTokens.FirstOrDefault(x => x.TelegramUserId == telegramUserId);
            if (userToken == null)
            {
                userToken = new UserToken
                {
                    TelegramUserId = telegramUserId,
                    RefreshToken = token.AccessToken
                };
                _db.UserTokens.Add(userToken);
                _logger.LogInformation("New user token added for user {UserId}", telegramUserId);
            }
            else
            {
                userToken.RefreshToken = token.AccessToken;
                _logger.LogInformation("Existing user token updated for user {UserId}", telegramUserId);
            }
            await _db.SaveChangesAsync();

            return Content("Авторизация успешна! Возвращайтесь к боту.");
        }
    }

}
