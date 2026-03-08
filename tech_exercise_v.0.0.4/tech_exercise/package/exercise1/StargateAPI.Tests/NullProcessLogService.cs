using StargateAPI.Business.Services;

namespace StargateAPI.Tests;

/// <summary>
/// No-op process log service for unit tests.
/// </summary>
public sealed class NullProcessLogService : IProcessLogService
{
    public static readonly NullProcessLogService Instance = new();

    public Task LogSuccessAsync(string requestPath, string requestMethod, int statusCode, string? message = null, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task LogExceptionAsync(Exception ex, string? requestPath = null, string? requestMethod = null, int? statusCode = null, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
