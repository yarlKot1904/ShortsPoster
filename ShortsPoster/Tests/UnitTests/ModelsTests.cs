using Xunit;
using ShortsPoster.Models;

public class ModelsTests
{
    [Fact]
    public void UserToken_ShouldSetProperties()
    {
        var token = new UserToken { UserId = "user", AccessToken = "access" };
        Assert.Equal("user", token.UserId);
        Assert.Equal("access", token.AccessToken);
    }
}
