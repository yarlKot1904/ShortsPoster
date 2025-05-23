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
        context.UserTokens.Add(new UserToken { UserId = "test", AccessToken = "token" });
        context.SaveChanges();
        Assert.Equal(1, context.UserTokens.Count());
    }
}
