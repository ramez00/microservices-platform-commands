using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PlatformService.Models;

namespace PlatformService.Data
{
    public class PlatformRepo(ApplicationDbContext dbContext) : IPlatformRepo
    {
        private readonly ApplicationDbContext _dbContext = dbContext;

        public void CreatePlatform(Platform platform)
        {
            if (platform is null)
                throw new ArgumentNullException(nameof(platform));
 
            _dbContext.Platforms.Add(platform);
        }

        public IEnumerable<Platform> GetAllPlatforms() => _dbContext.Platforms.ToList();

        public async Task<IEnumerable<Platform>> GetAllPlatformsAsync() => await _dbContext.Platforms.ToListAsync();

        public Platform? GetById(int id) => _dbContext.Platforms.FirstOrDefault(p => p.Id == id);

        public bool SaveChanges() => _dbContext.SaveChanges() >= 0;
    }

}