using Xunit;
using ShortsPoster.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Moq;

public class YouTubePosterTests
{
    [Fact]
    public void Constructor_CreatesInstance()
    {
        var services = new ServiceCollection();
        var loggerMock = new Mock<ILogger<YouTubePoster>>();
        services.AddSingleton(loggerMock.Object);
        var cfgMock = new Mock<IConfiguration>();
        services.AddSingleton(cfgMock.Object);
        var provider = services.BuildServiceProvider();

        var poster = new YouTubePoster(cfgMock.Object, provider);
        Assert.NotNull(poster);
    }
}
