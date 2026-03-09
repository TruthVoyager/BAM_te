using Microsoft.AspNetCore.Http;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using NUnit.Framework;

namespace StargateAPI.Tests.Commands;

[TestFixture]
public class CreatePersonTests
{
    [Test]
    public async Task Handle_WhenNameIsNew_CreatesPersonAndReturnsId()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        var handler = new CreatePersonHandler(context);

        var result = await handler.Handle(new CreatePerson { Name = "Alice" }, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.GreaterThan(0));
        var person = await context.People.FindAsync(result.Id);
        Assert.That(person, Is.Not.Null);
        Assert.That(person!.Name, Is.EqualTo("Alice"));
    }

    [Test]
    public void PreProcessor_WhenPersonWithNameExists_ThrowsBadRequest()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        context.People.Add(new Person { Name = "Bob" });
        context.SaveChanges();

        var preProcessor = new CreatePersonPreProcessor(context);
        var ex = Assert.ThrowsAsync<BadHttpRequestException>(async () =>
            await preProcessor.Process(new CreatePerson { Name = "Bob" }, CancellationToken.None));
        Assert.That(ex!.StatusCode, Is.EqualTo(StatusCodes.Status409Conflict));
    }

    [Test]
    public void PreProcessor_WhenPersonWithNameDoesNotExist_DoesNotThrow()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        var preProcessor = new CreatePersonPreProcessor(context);

        Assert.DoesNotThrowAsync(async () =>
            await preProcessor.Process(new CreatePerson { Name = "Charlie" }, CancellationToken.None));
    }
}
