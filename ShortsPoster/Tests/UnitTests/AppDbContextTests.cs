using Xunit;
using Microsoft.EntityFrameworkCore;
using ShortsPoster.Db;
using ShortsPoster.Models;
using System.Linq;

public class AppDbContextTests
{
    [Fact]
    public void CanInsertUserToken()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        using var context = new AppDbContext(options);
        context.UserTokens.Add(new UserToken { TelegramUserId = 123, RefreshToken = "token" });
        context.SaveChanges();
        Assert.Single(context.UserTokens);
        var userToken = context.UserTokens.First();
        Assert.Equal(123, userToken.TelegramUserId);
        Assert.Equal("token", userToken.RefreshToken);
    }

    [Fact]
    public void CanInsertUserLastTags()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb2")
            .Options;

        using var context = new AppDbContext(options);
        context.LastTags.Add(new UserLastTags { TelegramUserId = 222, TagsCsv = "tag1,tag2" });
        context.SaveChanges();
        Assert.Single(context.LastTags);
        var lastTags = context.LastTags.First();
        Assert.Equal(222, lastTags.TelegramUserId);
        Assert.Equal("tag1,tag2", lastTags.TagsCsv);
    }
}
