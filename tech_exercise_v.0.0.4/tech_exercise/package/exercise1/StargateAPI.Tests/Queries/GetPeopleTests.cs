using StargateAPI.Business.Data;
using StargateAPI.Business.Queries;
using NUnit.Framework;

namespace StargateAPI.Tests.Queries;

[TestFixture]
public class GetPeopleTests
{
    [Test]
    public async Task Handle_WhenNoPeople_ReturnsEmptyList()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        var handler = new GetPeopleHandler(context);

        var result = await handler.Handle(new GetPeople(), CancellationToken.None);

        Assert.That(result.People, Is.Not.Null.And.Empty);
    }

    [Test]
    public async Task Handle_WhenPeopleExist_ReturnsAllWithCurrentDutyDerived()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        TestContextFactory.SeedRanks(context);
        var p1 = await TestContextFactory.SeedPersonAsync(context, "Alpha");
        var p2 = await TestContextFactory.SeedPersonAsync(context, "Beta");
        context.AstronautDetails.Add(new AstronautDetail { PersonId = p1.Id, CareerStartDate = DateTime.Today.AddDays(-10) });
        context.AstronautDuties.Add(new AstronautDuty
        {
            PersonId = p1.Id,
            RankId = 2,
            DutyTitle = "Sergeant",
            DutyStartDate = DateTime.Today.AddDays(-5),
            DutyEndDate = null
        });
        await context.SaveChangesAsync();

        var handler = new GetPeopleHandler(context);
        var result = await handler.Handle(new GetPeople(), CancellationToken.None);

        Assert.That(result.People, Has.Count.EqualTo(2));
        var alpha = result.People.First(p => p.Name == "Alpha");
        Assert.That(alpha.PersonId, Is.EqualTo(p1.Id));
        Assert.That(alpha.CareerStartDate, Is.Not.Null);
        Assert.That(alpha.CurrentRank, Is.EqualTo("Sergeant"));
        Assert.That(alpha.CurrentDutyTitle, Is.EqualTo("Sergeant"));

        var beta = result.People.First(p => p.Name == "Beta");
        Assert.That(beta.CurrentRank, Is.Empty);
        Assert.That(beta.CurrentDutyTitle, Is.Empty);
    }
}
