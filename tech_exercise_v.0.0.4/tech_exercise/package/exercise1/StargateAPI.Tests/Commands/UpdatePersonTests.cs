using StargateAPI.Business.Commands;
using NUnit.Framework;

namespace StargateAPI.Tests.Commands;

[TestFixture]
public class UpdatePersonTests
{
    [Test]
    public async Task Handle_WhenPersonExists_UpdatesName()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        var person = await TestContextFactory.SeedPersonAsync(context, "OldName");
        var handler = new UpdatePersonHandler(context);

        var result = await handler.Handle(new UpdatePerson { CurrentName = "OldName", NewName = "NewName" }, CancellationToken.None);

        Assert.That(result.Id, Is.EqualTo(person.Id));
        var updated = await context.People.FindAsync(person.Id);
        Assert.That(updated!.Name, Is.EqualTo("NewName"));
    }

    [Test]
    public void Handle_WhenPersonNotFound_Throws()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        var handler = new UpdatePersonHandler(context);

        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await handler.Handle(new UpdatePerson { CurrentName = "Nobody", NewName = "NewName" }, CancellationToken.None));
        Assert.That(ex!.Message, Is.EqualTo("Person not found."));
    }

    [Test]
    public async Task Handle_WhenNewNameAlreadyExists_Throws()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        await TestContextFactory.SeedPersonAsync(context, "PersonA");
        await TestContextFactory.SeedPersonAsync(context, "PersonB");
        var handler = new UpdatePersonHandler(context);

        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await handler.Handle(new UpdatePerson { CurrentName = "PersonA", NewName = "PersonB" }, CancellationToken.None));
        Assert.That(ex!.Message, Is.EqualTo("A person with that name already exists."));
    }

    [Test]
    public async Task Handle_WhenNameUnchanged_DoesNotThrowAndSaves()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        var person = await TestContextFactory.SeedPersonAsync(context, "SameName");
        var handler = new UpdatePersonHandler(context);

        var result = await handler.Handle(new UpdatePerson { CurrentName = "SameName", NewName = "SameName" }, CancellationToken.None);

        Assert.That(result.Id, Is.EqualTo(person.Id));
        var updated = await context.People.FindAsync(person.Id);
        Assert.That(updated!.Name, Is.EqualTo("SameName"));
    }
}
