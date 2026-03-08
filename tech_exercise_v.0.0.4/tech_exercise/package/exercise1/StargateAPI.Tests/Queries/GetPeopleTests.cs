using FluentAssertions;
using StargateAPI.Business.Data;
using StargateAPI.Business.Queries;
using Xunit;

namespace StargateAPI.Tests.Queries;

public class GetPeopleTests
{
    [Fact]
    public async Task Handle_WhenNoPeople_ReturnsEmptyList()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        var handler = new GetPeopleHandler(context);

        var result = await handler.Handle(new GetPeople(), CancellationToken.None);

        result.People.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
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

        result.People.Should().HaveCount(2);
        var alpha = result.People.First(p => p.Name == "Alpha");
        alpha.PersonId.Should().Be(p1.Id);
        alpha.CareerStartDate.Should().NotBeNull();
        alpha.CurrentRank.Should().Be("Sergeant");
        alpha.CurrentDutyTitle.Should().Be("Sergeant");

        var beta = result.People.First(p => p.Name == "Beta");
        beta.CurrentRank.Should().BeEmpty();
        beta.CurrentDutyTitle.Should().BeEmpty();
    }
}
