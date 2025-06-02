using Xunit;
using ShortsPoster.Models;

public class ModelsTests
{
    [Fact]
    public void UserToken_Properties_AreSetCorrectly()
    {
        var dt = DateTime.UtcNow;
        var token = new UserToken { TelegramUserId = 42, RefreshToken = "xyz", CreatedAt = dt };
        Assert.Equal(42, token.TelegramUserId);
        Assert.Equal("xyz", token.RefreshToken);
        Assert.Equal(dt, token.CreatedAt);
    }

    [Fact]
    public void UserLastTags_Properties_AreSetCorrectly()
    {
        var lastTags = new UserLastTags { TelegramUserId = 10, TagsCsv = "a,b,c" };
        Assert.Equal(10, lastTags.TelegramUserId);
        Assert.Equal("a,b,c", lastTags.TagsCsv);
    }
}
