using Xunit;
using ShortsPoster.Util;

public class YouTubePosterTests
{
    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        var poster = new YouTubePoster();
        Assert.NotNull(poster);
    }

    [Fact]
    public async Task PostVideoAsync_ShouldThrowIfInvalid()
    {
        var poster = new YouTubePoster();
        await Assert.ThrowsAsync<ArgumentException>(() =>
            poster.PostVideoAsync(null, "", "", ""));
    }
}
