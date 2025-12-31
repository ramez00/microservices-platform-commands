    using System;
    using System.Threading.Tasks;
    using PlatformService.SyncDataServices.Grpc;
    using PlatformService.Data;
    using AutoMapper;
    using Grpc.Core;

namespace PlatformService.SyncDataServices.Grpc
{

    public class GrpcPlatformService : GrpcPlatform.GrpcPlatformBase
    {
        private readonly IPlatformRepo _platfromRepo; 
        private readonly IMapper _mapper;

       public GrpcPlatformService(IPlatformRepo  platfromRepo, IMapper mapper)
        {
            _platfromRepo = platfromRepo;
            _mapper = mapper;
        }

        public override Task<PlatformResponse> GetAllPlatforms(GetAllRequest request, ServerCallContext context)
        {
            var response = new PlatformResponse();
            var platforms = _platfromRepo.GetAllPlatforms();

            foreach (var platform in platforms)
                response.Platform.Add(_mapper.Map<GrpcPlatformModel>(platform));

            return Task.FromResult(response);
        }
    }
}