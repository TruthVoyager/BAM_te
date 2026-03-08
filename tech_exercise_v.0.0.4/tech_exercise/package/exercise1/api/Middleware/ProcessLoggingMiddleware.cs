using StargateAPI.Business.Services;

namespace StargateAPI.Middleware
{
    public class ProcessLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public ProcessLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IProcessLogService processLog)
        {
            await _next(context);

            var statusCode = context.Response.StatusCode;
            if (statusCode >= 200 && statusCode < 300)
            {
                try
                {
                    await processLog.LogSuccessAsync(
                        context.Request.Path,
                        context.Request.Method,
                        statusCode,
                        cancellationToken: context.RequestAborted);
                }
                catch
                {
                    // Do not let logging failure affect the response
                }
            }
        }
    }
}
