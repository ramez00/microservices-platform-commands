using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.Data;
using PlatformService.Dtos;
using PlatformService.Models;
using PlatformService.SyncDataServices.Http;
using PlatformService.AsyncDataServices;

namespace PlatformService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlatformController(ICommandDataClient commandData,IPlatformRepo repo, IMapper mapper,IMessageBusClient messageBus) : ControllerBase
    {
        private readonly IPlatformRepo _repo = repo;
        private readonly IMapper _mapper = mapper;
        private readonly ICommandDataClient _commandData = commandData; 

        private readonly IMessageBusClient _messageBus = messageBus;

        [HttpGet("testApi")]
        public IActionResult TestApi() => Ok("This is a test API from PlatformService");
        
        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(_mapper.Map<IEnumerable<PlatformDto>>(await _repo.GetAllPlatformsAsync()));

        [HttpGet("{id}")]
        public IActionResult getById(int id)
        {
            var platform = _repo.GetById(id);

            if (platform is null)
                return NotFound();

            return Ok(_mapper.Map<PlatformDto>(platform));
        }

        [HttpPost("AddNewPlatform")]
        public IActionResult Add(PlatformDto dto)
        {
            var platform = _mapper.Map<Platform>(dto);
            _repo.CreatePlatform(platform);
            _repo.SaveChanges();

            var platformReadDto = _mapper.Map<PlatformDto>(platform);

            return CreatedAtAction(nameof(getById), new { Id = platformReadDto.Id }, platformReadDto);
        }

        [HttpPost("CreatePlatForm")]
        public async Task<IActionResult> CreatePlatForm(PlatformDto dto)
        {
            var platform = _mapper.Map<Platform>(dto);
            _repo.CreatePlatform(platform);
            _repo.SaveChanges();
            
            // Send Sync Message
            try
            {
                Console.WriteLine("===> inside Sending Sync Message to Command Service");
                await _commandData.SendPlatformToCommand(dto);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"==> Error Occured in PlatFormService Controller : {ex.Message}");
            }
            // Send Async Message
            try
            {
                var platformPublishedDto = _mapper.Map<PlatformPublishedDto>(platform);
                platformPublishedDto.Event = "Platform_Published";
                _messageBus.PublishNewPlatform(platformPublishedDto);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"==> Could not send async message: {ex.Message}");
                    throw; // 🔥 IMPORTANT

            }

            return Created();
        }
                
    }
}