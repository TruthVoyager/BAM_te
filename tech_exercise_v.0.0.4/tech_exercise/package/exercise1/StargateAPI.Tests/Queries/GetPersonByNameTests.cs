using FluentAssertions;
using StargateAPI.Business.Data;
using StargateAPI.Business.Queries;
using Xunit;

namespace StargateAPI.Tests.Queries;

public class GetPersonByNameTests
{
    [Fact]
    public async Task Handle_WhenPersonNotFound_ReturnsNullPerson()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        var handler = new GetPersonByNameHandler(context);

        var result = await handler.Handle(new GetPersonByName { Name = "Nobody" }, CancellationToken.None);

        result.Person.Should().BeNull();
    }

    [Fact]
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

        result.Person.Should().NotBeNull();
        result.Person!.Name.Should().Be("Jane");
        result.Person.CurrentRank.Should().Be("Captain");
        result.Person.CurrentDutyTitle.Should().Be("Captain");
    }
}
