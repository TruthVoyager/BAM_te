using StargateAPI.Business.Data;

namespace StargateAPI.Business.Services
{
    public class ProcessLogService : IProcessLogService
    {
        private readonly StargateContext _context;

        public ProcessLogService(StargateContext context)
        {
            _context = context;
        }

        public async Task LogSuccessAsync(string requestPath, string requestMethod, int statusCode, string? message = null, CancellationToken cancellationToken = default)
        {
            var log = new ProcessLog
            {
                CreatedAtUtc = DateTime.UtcNow,
                Level = "Success",
                Message = message ?? $"Request completed successfully.",
                RequestPath = requestPath,
                RequestMethod = requestMethod,
                StatusCode = statusCode
            };
            _context.ProcessLogs.Add(log);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task LogExceptionAsync(Exception ex, string? requestPath = null, string? requestMethod = null, int? statusCode = null, CancellationToken cancellationToken = default)
        {
            var log = new ProcessLog
            {
                CreatedAtUtc = DateTime.UtcNow,
                Level = "Error",
                Message = ex.Message,
                RequestPath = requestPath,
                RequestMethod = requestMethod,
                StatusCode = statusCode,
                ExceptionMessage = ex.Message,
                ExceptionStackTrace = ex.StackTrace
            };
            _context.ProcessLogs.Add(log);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
