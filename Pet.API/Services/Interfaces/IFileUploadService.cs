namespace Pet.API.Services.Interfaces
{
    public interface IFileUploadService
    {
        Task<string> UploadImageAsync(Stream fileStream, string fileName, string? folder = null);
    }
}

