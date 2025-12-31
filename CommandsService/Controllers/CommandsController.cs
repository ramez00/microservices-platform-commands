namespace CommandsService.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using CommandsService.Data;
    using CommandsService.Dtos;
    using AutoMapper;
    using System;
    using System.Collections.Generic;

    [ApiController]
    [Route("api/c/platforms/{platformId}/[controller]")]
    public class CommandsController(ICommandRepo commandRepo, IMapper mapper) : ControllerBase
    {
        private readonly ICommandRepo _commandRepo = commandRepo;
        private readonly IMapper _mapper = mapper;

        [HttpGet]
        public ActionResult<IEnumerable<CommandDto>> Get(int platformId)
        {
            Console.WriteLine($"--> Hit Getting Commands for Platform => {platformId} from CommandsService");

            if(!_commandRepo.PlatformExists(platformId))
                return NotFound();
            
            var commandItems = _commandRepo.GetCommandsForPlatform(platformId);

            return Ok(_mapper.Map<IEnumerable<CommandDto>>(commandItems));
        }

        [HttpGet("{commandId}",Name = "GetCommandForPlatform")]
        public ActionResult<CommandDto> GetCommandForPlatform(int platformId, int commandId)
        {
            Console.WriteLine($"--> Hit GetCommandForPlatform => {platformId} / {commandId} from CommandsService");
            
            if(!_commandRepo.PlatformExists(platformId))
                return NotFound();

            var commandItem = _commandRepo.GetCommand(platformId, commandId);

            if (commandItem is null)
                return NotFound();

            return Ok(_mapper.Map<CommandDto>(commandItem));
        }

        [HttpPost]
        public ActionResult<CommandDto> CreateCommandForPlatform(int platformId, [FromBody] CommandDto commandCreateDto)
        {
            Console.WriteLine($"--> Hit CreateCommandForPlatform => {platformId} from CommandsService");

            if(!_commandRepo.PlatformExists(platformId))
                return NotFound();

            var commandModel = _mapper.Map<Models.Command>(commandCreateDto);
            _commandRepo.CreateCommand(platformId, commandModel);
            _commandRepo.SaveChanges();

            var commandReadDto = _mapper.Map<CommandDto>(commandModel);

            return CreatedAtRoute(nameof(GetCommandForPlatform), 
            new { platformId = platformId, commandId = commandReadDto.Id }, commandReadDto);
        }
    }
}