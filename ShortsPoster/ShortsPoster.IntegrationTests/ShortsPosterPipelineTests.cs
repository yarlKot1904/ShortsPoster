using Xunit;
using System;
using System.Threading;
using ShortsPoster.Util;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

public class ShortsPosterPipeLineTests
{
    [Fact]
    public async Task UploadAsync_ThrowsIfUserNotAuthorized()
    {
        var cfg = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(cfg);
        var provider = services.BuildServiceProvider();

        var poster = new YouTubePoster(cfg, provider);
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await poster.UploadAsync(123, "not_exists.mp4", "title", "desc", Array.Empty<string>(), CancellationToken.None);
        });
    }
}
