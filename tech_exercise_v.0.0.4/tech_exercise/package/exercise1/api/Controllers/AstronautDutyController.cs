using MediatR;
using Microsoft.AspNetCore.Mvc;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Queries;
using StargateAPI.Business.Services;

namespace StargateAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AstronautDutyController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AstronautDutyController> _logger;
        private readonly IProcessLogService _processLog;

        public AstronautDutyController(IMediator mediator, ILogger<AstronautDutyController> logger, IProcessLogService processLog)
        {
            _mediator = mediator;
            _logger = logger;
            _processLog = processLog;
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> GetAstronautDutiesByName(string name)
        {
            try
            {
                var result = await _mediator.Send(new GetAstronautDutiesByName() { Name = name });

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
        public async Task<IActionResult> CreateAstronautDuty([FromBody] CreateAstronautDuty request)
        {
            try
            {
                var result = await _mediator.Send(request);
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