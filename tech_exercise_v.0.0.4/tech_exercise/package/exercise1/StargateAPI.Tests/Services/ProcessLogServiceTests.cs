using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Services;
using NUnit.Framework;

namespace StargateAPI.Tests.Services;

[TestFixture]
public class ProcessLogServiceTests
{
    [Test]
    public async Task LogSuccessAsync_PersistsLogToDatabase()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        var sut = new ProcessLogService(context);

        await sut.LogSuccessAsync("/api/Person", "GET", 200, "OK");

        var log = await context.ProcessLogs.AsNoTracking().SingleAsync();
        Assert.That(log.Level, Is.EqualTo("Success"));
        Assert.That(log.RequestPath, Is.EqualTo("/api/Person"));
        Assert.That(log.RequestMethod, Is.EqualTo("GET"));
        Assert.That(log.StatusCode, Is.EqualTo(200));
        Assert.That(log.Message, Is.EqualTo("OK"));
        Assert.That(log.ExceptionMessage, Is.Null);
    }

    [Test]
    public async Task LogSuccessAsync_WhenMessageNull_UsesDefaultMessage()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        var sut = new ProcessLogService(context);

        await sut.LogSuccessAsync("/test", "POST", 201, null);

        var log = await context.ProcessLogs.AsNoTracking().SingleAsync();
        Assert.That(log.Message, Is.EqualTo("Request completed successfully."));
    }

    [Test]
    public async Task LogExceptionAsync_PersistsExceptionToDatabase()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        var sut = new ProcessLogService(context);
        var ex = new InvalidOperationException("Person not found.");

        await sut.LogExceptionAsync(ex, "/api/Person/John", "GET", 404);

        var log = await context.ProcessLogs.AsNoTracking().SingleAsync();
        Assert.That(log.Level, Is.EqualTo("Error"));
        Assert.That(log.RequestPath, Is.EqualTo("/api/Person/John"));
        Assert.That(log.RequestMethod, Is.EqualTo("GET"));
        Assert.That(log.StatusCode, Is.EqualTo(404));
        Assert.That(log.Message, Is.EqualTo("Person not found."));
        Assert.That(log.ExceptionMessage, Is.EqualTo("Person not found."));
        if (ex.StackTrace is not null)
            Assert.That(log.ExceptionStackTrace, Is.Not.Null.And.Not.Empty);
    }
}
