using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Services;
using Xunit;

namespace StargateAPI.Tests.Services;

public class ProcessLogServiceTests
{
    [Fact]
    public async Task LogSuccessAsync_PersistsLogToDatabase()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        var sut = new ProcessLogService(context);

        await sut.LogSuccessAsync("/api/Person", "GET", 200, "OK");

        var log = await context.ProcessLogs.AsNoTracking().SingleAsync();
        log.Level.Should().Be("Success");
        log.RequestPath.Should().Be("/api/Person");
        log.RequestMethod.Should().Be("GET");
        log.StatusCode.Should().Be(200);
        log.Message.Should().Be("OK");
        log.ExceptionMessage.Should().BeNull();
    }

    [Fact]
    public async Task LogSuccessAsync_WhenMessageNull_UsesDefaultMessage()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        var sut = new ProcessLogService(context);

        await sut.LogSuccessAsync("/test", "POST", 201, null);

        var log = await context.ProcessLogs.AsNoTracking().SingleAsync();
        log.Message.Should().Be("Request completed successfully.");
    }

    [Fact]
    public async Task LogExceptionAsync_PersistsExceptionToDatabase()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        var sut = new ProcessLogService(context);
        var ex = new InvalidOperationException("Person not found.");

        await sut.LogExceptionAsync(ex, "/api/Person/John", "GET", 404);

        var log = await context.ProcessLogs.AsNoTracking().SingleAsync();
        log.Level.Should().Be("Error");
        log.RequestPath.Should().Be("/api/Person/John");
        log.RequestMethod.Should().Be("GET");
        log.StatusCode.Should().Be(404);
        log.Message.Should().Be("Person not found.");
        log.ExceptionMessage.Should().Be("Person not found.");
        if (ex.StackTrace is not null)
            log.ExceptionStackTrace.Should().NotBeNullOrEmpty();
    }
}
