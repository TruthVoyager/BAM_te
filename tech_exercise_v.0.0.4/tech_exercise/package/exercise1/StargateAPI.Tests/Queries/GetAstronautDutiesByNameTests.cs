using StargateAPI.Business.Data;
using StargateAPI.Business.Queries;
using NUnit.Framework;

namespace StargateAPI.Tests.Queries;

[TestFixture]
public class GetAstronautDutiesByNameTests
{
    [Test]
    public async Task Handle_WhenPersonNotFound_ReturnsNullPersonAndEmptyDuties()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        var handler = new GetAstronautDutiesByNameHandler(context);

        var result = await handler.Handle(new GetAstronautDutiesByName { Name = "Nobody" }, CancellationToken.None);

        Assert.That(result.Person, Is.Null);
        Assert.That(result.AstronautDuties, Is.Not.Null.And.Empty);
    }

    [Test]
    public async Task Handle_WhenPersonExists_ReturnsPersonAndDutiesOrderedByStartDateDescending()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        TestContextFactory.SeedRanks(context);
        var person = await TestContextFactory.SeedPersonAsync(context, "DutyPerson");
        context.AstronautDetails.Add(new AstronautDetail { PersonId = person.Id, CareerStartDate = DateTime.Today.AddDays(-20) });
        context.AstronautDuties.AddRange(
            new AstronautDuty
            {
                PersonId = person.Id,
                RankId = 1,
                DutyTitle = "First",
                DutyStartDate = DateTime.Today.AddDays(-20),
                DutyEndDate = DateTime.Today.AddDays(-11)
            },
            new AstronautDuty
            {
                PersonId = person.Id,
                RankId = 2,
                DutyTitle = "Second",
                DutyStartDate = DateTime.Today.AddDays(-10),
                DutyEndDate = null
            }
        );
        await context.SaveChangesAsync();

        var handler = new GetAstronautDutiesByNameHandler(context);
        var result = await handler.Handle(new GetAstronautDutiesByName { Name = "DutyPerson" }, CancellationToken.None);

        Assert.That(result.Person, Is.Not.Null);
        Assert.That(result.Person!.Name, Is.EqualTo("DutyPerson"));
        Assert.That(result.Person.CurrentRank, Is.EqualTo("Sergeant"));
        Assert.That(result.Person.CurrentDutyTitle, Is.EqualTo("Second"));

        Assert.That(result.AstronautDuties, Has.Count.EqualTo(2));
        Assert.That(result.AstronautDuties[0].DutyTitle, Is.EqualTo("Second"));
        Assert.That(result.AstronautDuties[0].DutyEndDate, Is.Null);
        Assert.That(result.AstronautDuties[1].DutyTitle, Is.EqualTo("First"));
        Assert.That(result.AstronautDuties[1].DutyEndDate, Is.Not.Null);
        Assert.That(result.AstronautDuties[1].RankLevel, Is.EqualTo(1));
    }
}
