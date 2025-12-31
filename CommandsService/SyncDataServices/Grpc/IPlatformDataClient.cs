using System.Threading;
using System.Threading.Tasks;
using CommandsService.SyncDataServices.Grpc;
using System.Collections.Generic;
using CommandsService.Models;

namespace CommandsService.SyncDataServices.Grpc
{
    public interface IPlatformDataClient
    {
        IEnumerable<Platform> ReturnAllPlatforms();
    }
}