using Xunit;
using Microsoft.AspNetCore.Mvc;
using ShortsPoster.Controllers;

public class OAuthControllerTests
{
    [Fact]
    public void GetAuthUrl_ShouldReturnUrl()
    {
        var controller = new OAuthController();
        var result = controller.GetAuthUrl();
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Contains("https://", okResult.Value.ToString());
    }
}
