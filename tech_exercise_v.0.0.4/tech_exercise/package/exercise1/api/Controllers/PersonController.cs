using MediatR;
using Microsoft.AspNetCore.Mvc;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Queries;
using StargateAPI.Business.Services;

namespace StargateAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PersonController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PersonController> _logger;
        private readonly IProcessLogService _processLog;

        public PersonController(IMediator mediator, ILogger<PersonController> logger, IProcessLogService processLog)
        {
            _mediator = mediator;
            _logger = logger;
            _processLog = processLog;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetPeople()
        {
            try
            {
                var result = await _mediator.Send(new GetPeople() { });

                return this.GetResponse(result);
            }
            catch (Exception ex)
            {
                var response = ex.ToSafeResponse(_logger);
                await _processLog.LogExceptionAsync(ex, HttpContext.Request.Path, HttpContext.Request.Method, response.ResponseCode);
                return this.GetResponse(response);
            }
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> GetPersonByName(string name)
        {
            try
            {
                var result = await _mediator.Send(new GetPersonByName() { Name = name });

                return this.GetResponse(result);
            }
            catch (Exception ex)
            {
                var response = ex.ToSafeResponse(_logger);
                await _processLog.LogExceptionAsync(ex, HttpContext.Request.Path, HttpContext.Request.Method, response.ResponseCode);
                return this.GetResponse(response);
            }
        }

        [HttpPost("")]
        public async Task<IActionResult> CreatePerson([FromBody] string name)
        {
            try
            {
                var result = await _mediator.Send(new CreatePerson() { Name = name });

                return this.GetResponse(result);
            }
            catch (Exception ex)
            {
                var response = ex.ToSafeResponse(_logger);
                await _processLog.LogExceptionAsync(ex, HttpContext.Request.Path, HttpContext.Request.Method, response.ResponseCode);
                return this.GetResponse(response);
            }
        }

        [HttpPut("{currentName}")]
        public async Task<IActionResult> UpdatePerson(string currentName, [FromBody] string newName)
        {
            try
            {
                var result = await _mediator.Send(new UpdatePerson()
                {
                    CurrentName = currentName,
                    NewName = newName ?? string.Empty
                });

                return this.GetResponse(result);
            }
            catch (Exception ex)
            {
                var response = ex.ToSafeResponse(_logger);
                await _processLog.LogExceptionAsync(ex, HttpContext.Request.Path, HttpContext.Request.Method, response.ResponseCode);
                return this.GetResponse(response);
            }
        }
    }
}