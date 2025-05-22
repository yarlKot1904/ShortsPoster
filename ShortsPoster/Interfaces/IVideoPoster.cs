namespace ShortsPoster.Interfaces
{
    public interface IVideoPoster
    {
        Task<string> UploadAsync(long tgUserId, string videoPath, string title, string description, string[] tags, CancellationToken ct);
    }

}
