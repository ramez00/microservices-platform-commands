using Microsoft.AspNetCore.Mvc;
using CommandsService.Data;
using CommandsService.Dtos;
using AutoMapper;

namespace CommandsService.Controllers
{
    [ApiController]
    [Route("api/c/[controller]")]
    public class PlatformsController(ICommandRepo commandRepo,IMapper mapper) : ControllerBase
    {
        private readonly ICommandRepo _commandRepo = commandRepo;
        private readonly IMapper _mapper = mapper;

        [HttpGet("platforms")]
        public ActionResult<IEnumerable<PlatformDto>> GetPlatforms()
        {
            Console.WriteLine("--> Getting Platforms from CommandsService");

            var platformItems = _commandRepo.GetAllPlatforms();

            return Ok(_mapper.Map<IEnumerable<PlatformDto>>(platformItems));
        }

        [HttpGet]
        public ActionResult Get()
        {
            return Ok("Test API From Main Get of Controller");
        }
        
        [HttpGet("testAPI")]
        public ActionResult testAPI()
        {
            return Ok("Test API"); 
        }
        [HttpPost("testPost")]
        public ActionResult TestInboundConnection()
        {
            Console.WriteLine("----> Inbound POST # Command Service");

            return Ok("Inbound test of Platforms Controller"); 
        }
    }
}
