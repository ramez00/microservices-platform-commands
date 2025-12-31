using CommandsService.Models;
using CommandsService.Data;
using System.Collections.Generic;
using System.Linq;

namespace CommandsService.Data
{
    public class CommandRepo(AppDbContext context) : ICommandRepo
    {
        private readonly AppDbContext _context = context;

         public bool SaveChanges() => (_context.SaveChanges() >= 0); 

        // Platforms
        public IEnumerable<Platform> GetAllPlatforms() => _context.Platforms.ToList();

        public void CreatePlatform(Platform plat)
        {
            if (plat is null)
                throw new ArgumentNullException(nameof(plat));
            
            _context.Platforms.Add(plat);
        }

        public bool PlatformExists(int platformId) => _context.Platforms.Any(p => p.Id == platformId);

        // Commands
        public IEnumerable<Command> GetCommandsForPlatform(int platformId)
        {
            return _context.Commands
                .Where(c => c.PlatformId == platformId)
                .OrderBy(c => c.PlatformId)
                .ToList();

        }

        public Command GetCommand(int platformId, int commandId)
         => _context.Commands.FirstOrDefault(c => c.PlatformId == platformId && c.Id == commandId);


        public void CreateCommand(int platformId, Command command)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));
            
            command.PlatformId = platformId;
            _context.Commands.Add(command);
        }

        public bool ExternalPlatformExists(int externalPlatformId)
            => _context.Platforms.Any(p => p.ExternalID == externalPlatformId);
    }
}