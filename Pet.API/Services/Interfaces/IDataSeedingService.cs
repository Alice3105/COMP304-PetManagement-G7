namespace Pet.API.Services.Interfaces
{
    public interface IDataSeedingService
    {
        Task SeedDataAsync();
        Task<bool> IsDataSeededAsync();
    }
}

