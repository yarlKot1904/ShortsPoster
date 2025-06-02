using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ShortsPoster.Controllers;
using ShortsPoster.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

public class OAuthControllerTests
{
    [Fact]
    public void Callback_Returns_BadRequest_When_Code_Or_State_IsMissing()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase("OAuthTestDb").Options;
        using var db = new AppDbContext(options);

        var loggerMock = new Mock<ILogger<OAuthController>>();
        var configMock = new Mock<IConfiguration>();

        var controller = new ShortsPoster.Controllers.OAuthController(db, configMock.Object, loggerMock.Object);

        var result1 = controller.Callback(null, "state").Result;
        var result2 = controller.Callback("code", null).Result;

        Assert.IsType<BadRequestObjectResult>(result1);
        Assert.IsType<BadRequestObjectResult>(result2);
    }
}
