namespace StargateAPI.Business.Services
{
    public interface IProcessLogService
    {
        Task LogSuccessAsync(string requestPath, string requestMethod, int statusCode, string? message = null, CancellationToken cancellationToken = default);
        Task LogExceptionAsync(Exception ex, string? requestPath = null, string? requestMethod = null, int? statusCode = null, CancellationToken cancellationToken = default);
    }
}
