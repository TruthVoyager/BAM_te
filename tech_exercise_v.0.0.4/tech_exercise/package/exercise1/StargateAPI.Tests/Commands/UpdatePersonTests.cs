using FluentAssertions;
using StargateAPI.Business.Commands;
using Xunit;

namespace StargateAPI.Tests.Commands;

public class UpdatePersonTests
{
    [Fact]
    public async Task Handle_WhenPersonExists_UpdatesName()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        var person = await TestContextFactory.SeedPersonAsync(context, "OldName");
        var handler = new UpdatePersonHandler(context);

        var result = await handler.Handle(new UpdatePerson { CurrentName = "OldName", NewName = "NewName" }, CancellationToken.None);

        result.Id.Should().Be(person.Id);
        var updated = await context.People.FindAsync(person.Id);
        updated!.Name.Should().Be("NewName");
    }

    [Fact]
    public async Task Handle_WhenPersonNotFound_Throws()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        var handler = new UpdatePersonHandler(context);

        var act = () => handler.Handle(new UpdatePerson { CurrentName = "Nobody", NewName = "NewName" }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Person not found.");
    }

    [Fact]
    public async Task Handle_WhenNewNameAlreadyExists_Throws()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        await TestContextFactory.SeedPersonAsync(context, "PersonA");
        await TestContextFactory.SeedPersonAsync(context, "PersonB");
        var handler = new UpdatePersonHandler(context);

        var act = () => handler.Handle(new UpdatePerson { CurrentName = "PersonA", NewName = "PersonB" }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("A person with that name already exists.");
    }

    [Fact]
    public async Task Handle_WhenNameUnchanged_DoesNotThrowAndSaves()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        var person = await TestContextFactory.SeedPersonAsync(context, "SameName");
        var handler = new UpdatePersonHandler(context);

        var result = await handler.Handle(new UpdatePerson { CurrentName = "SameName", NewName = "SameName" }, CancellationToken.None);

        result.Id.Should().Be(person.Id);
        var updated = await context.People.FindAsync(person.Id);
        updated!.Name.Should().Be("SameName");
    }
}
