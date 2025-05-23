using Xunit;
using ShortsPoster.Services;
using System.Threading.Tasks;
using System.IO;

namespace ShortsPoster.IntegrationTests
{
    public class ShortsPosterPipelineTests
    {
        [Fact]
        public async Task Pipeline_ProcessesTelegramVideoAndUploadsToYouTube()
        {
            
            var testVideoPath = Path.Combine("test_data", "test_video.mp4");
            Assert.True(File.Exists(testVideoPath), "Test video file is missing.");

            var telegramService = new TelegramService();      
            var processor = new VideoProcessor();             
            var youtubeUploader = new YouTubeUploader();      

            
            var messageId = await telegramService.UploadTestVideoAsync(testVideoPath);
            Assert.NotNull(messageId);

            var result = await processor.ProcessAsync(messageId);
            Assert.True(result.IsSuccess);

            var uploadResult = await youtubeUploader.UploadAsync(
                result.OutputFilePath,
                "Test Title",
                "Test Description"
            );

            
            Assert.True(uploadResult.IsSuccess);
            Assert.False(string.IsNullOrWhiteSpace(uploadResult.VideoId));
        }
    }
}
