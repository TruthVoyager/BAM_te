using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Services;

namespace StargateAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RankController : ControllerBase
    {
        private readonly StargateContext _context;
        private readonly ILogger<RankController> _logger;
        private readonly IProcessLogService _processLog;

        public RankController(StargateContext context, ILogger<RankController> logger, IProcessLogService processLog)
        {
            _context = context;
            _logger = logger;
            _processLog = processLog;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetRanks()
        {
            try
            {
                var ranks = await _context.Ranks.OrderBy(r => r.Level).ToListAsync();
                return Ok(ranks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load ranks: {Message}", ex.Message);
                await _processLog.LogExceptionAsync(ex, HttpContext.Request.Path, HttpContext.Request.Method, 500);
                return StatusCode(500, new { message = "An error occurred while loading ranks." });
            }
        }
    }
}
