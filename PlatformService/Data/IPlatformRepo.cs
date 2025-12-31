using PlatformService.Models;

namespace PlatformService.Data
{
    public interface IPlatformRepo
    {
        bool SaveChanges();

        IEnumerable<Platform> GetAllPlatforms();

        Task<IEnumerable<Platform>> GetAllPlatformsAsync();

        Platform? GetById(int id);

        void CreatePlatform(Platform platform);
        
    }
}