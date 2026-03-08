using FluentAssertions;
using StargateAPI.Business.Data;
using StargateAPI.Business.Queries;
using Xunit;

namespace StargateAPI.Tests.Queries;

public class GetAstronautDutiesByNameTests
{
    [Fact]
    public async Task Handle_WhenPersonNotFound_ReturnsNullPersonAndEmptyDuties()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        var handler = new GetAstronautDutiesByNameHandler(context);

        var result = await handler.Handle(new GetAstronautDutiesByName { Name = "Nobody" }, CancellationToken.None);

        result.Person.Should().BeNull();
        result.AstronautDuties.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
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

        result.Person.Should().NotBeNull();
        result.Person!.Name.Should().Be("DutyPerson");
        result.Person.CurrentRank.Should().Be("Sergeant");
        result.Person.CurrentDutyTitle.Should().Be("Second");

        result.AstronautDuties.Should().HaveCount(2);
        result.AstronautDuties[0].DutyTitle.Should().Be("Second");
        result.AstronautDuties[0].DutyEndDate.Should().BeNull();
        result.AstronautDuties[1].DutyTitle.Should().Be("First");
        result.AstronautDuties[1].DutyEndDate.Should().NotBeNull();
        result.AstronautDuties[1].RankLevel.Should().Be(1);
    }
}
