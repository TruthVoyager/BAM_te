using FluentAssertions;
using Microsoft.AspNetCore.Http;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using Xunit;

namespace StargateAPI.Tests.Commands;

public class CreatePersonTests
{
    [Fact]
    public async Task Handle_WhenNameIsNew_CreatesPersonAndReturnsId()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        var handler = new CreatePersonHandler(context);

        var result = await handler.Handle(new CreatePerson { Name = "Alice" }, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        var person = await context.People.FindAsync(result.Id);
        person.Should().NotBeNull();
        person!.Name.Should().Be("Alice");
    }

    [Fact]
    public async Task PreProcessor_WhenPersonWithNameExists_ThrowsBadRequest()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        context.People.Add(new Person { Name = "Bob" });
        context.SaveChanges();

        var preProcessor = new CreatePersonPreProcessor(context);
        var act = () => preProcessor.Process(new CreatePerson { Name = "Bob" }, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<BadHttpRequestException>();
        ex.Which.StatusCode.Should().Be(StatusCodes.Status409Conflict);
    }

    [Fact]
    public async Task PreProcessor_WhenPersonWithNameDoesNotExist_DoesNotThrow()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        var preProcessor = new CreatePersonPreProcessor(context);

        await preProcessor.Invoking(p => p.Process(new CreatePerson { Name = "Charlie" }, CancellationToken.None))
            .Should().NotThrowAsync();
    }
}
