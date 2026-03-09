using StargateAPI.Business.Data;
using StargateAPI.Business.Queries;
using NUnit.Framework;

namespace StargateAPI.Tests.Queries;

[TestFixture]
public class GetPersonByNameTests
{
    [Test]
    public async Task Handle_WhenPersonNotFound_ReturnsNullPerson()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        var handler = new GetPersonByNameHandler(context);

        var result = await handler.Handle(new GetPersonByName { Name = "Nobody" }, CancellationToken.None);

        Assert.That(result.Person, Is.Null);
    }

    [Test]
    public async Task Handle_WhenPersonExists_ReturnsPersonWithCurrentDuty()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        TestContextFactory.SeedRanks(context);
        var person = await TestContextFactory.SeedPersonAsync(context, "Jane");
        context.AstronautDetails.Add(new AstronautDetail { PersonId = person.Id, CareerStartDate = DateTime.Today.AddDays(-10) });
        context.AstronautDuties.Add(new AstronautDuty
        {
            PersonId = person.Id,
            RankId = 3,
            DutyTitle = "Captain",
            DutyStartDate = DateTime.Today.AddDays(-1),
            DutyEndDate = null
        });
        await context.SaveChangesAsync();

        var handler = new GetPersonByNameHandler(context);
        var result = await handler.Handle(new GetPersonByName { Name = "Jane" }, CancellationToken.None);

        Assert.That(result.Person, Is.Not.Null);
        Assert.That(result.Person!.Name, Is.EqualTo("Jane"));
        Assert.That(result.Person.CurrentRank, Is.EqualTo("Captain"));
        Assert.That(result.Person.CurrentDutyTitle, Is.EqualTo("Captain"));
    }
}
