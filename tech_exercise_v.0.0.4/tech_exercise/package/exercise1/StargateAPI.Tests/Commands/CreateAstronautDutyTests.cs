using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using Xunit;

namespace StargateAPI.Tests.Commands;

public class CreateAstronautDutyTests
{
    private static readonly DateTime Today = DateTime.Today;
    private static readonly DateTime Yesterday = Today.AddDays(-1);

    [Fact]
    public async Task PreProcessor_WhenPersonNotFound_ThrowsBadRequest()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        TestContextFactory.SeedRanks(context);
        var preProcessor = new CreateAstronautDutyPreProcessor(context);

        var act = () => preProcessor.Process(new CreateAstronautDuty
        {
            Name = "NoSuchPerson",
            RankId = 1,
            DutyTitle = "Captain",
            DutyStartDate = Today
        }, CancellationToken.None);

        await act.Should().ThrowAsync<BadHttpRequestException>();
    }

    [Fact]
    public async Task PreProcessor_WhenDuplicateDutyForSamePerson_ThrowsBadRequest()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        TestContextFactory.SeedRanks(context);
        var person = await TestContextFactory.SeedPersonAsync(context, "Dup");
        context.AstronautDuties.Add(new AstronautDuty
        {
            PersonId = person.Id,
            RankId = 1,
            DutyTitle = "Captain",
            DutyStartDate = Today,
            DutyEndDate = null
        });
        await context.SaveChangesAsync();

        var preProcessor = new CreateAstronautDutyPreProcessor(context);
        var act = () => preProcessor.Process(new CreateAstronautDuty
        {
            Name = "Dup",
            RankId = 1,
            DutyTitle = "Captain",
            DutyStartDate = Today
        }, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<BadHttpRequestException>();
        ex.Which.Message.Should().Contain("already has a duty with this title and start date");
    }

    [Fact]
    public async Task Handle_WhenPersonNotFound_Throws()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        TestContextFactory.SeedRanks(context);
        var handler = new CreateAstronautDutyHandler(context);

        var act = () => handler.Handle(new CreateAstronautDuty
        {
            Name = "NoOne",
            RankId = 1,
            DutyTitle = "Captain",
            DutyStartDate = Today
        }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Person not found.");
    }

    [Fact]
    public async Task Handle_WhenStartDateInFuture_ThrowsBadRequest()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        TestContextFactory.SeedRanks(context);
        await TestContextFactory.SeedPersonAsync(context, "Future");
        var handler = new CreateAstronautDutyHandler(context);

        var act = () => handler.Handle(new CreateAstronautDuty
        {
            Name = "Future",
            RankId = 1,
            DutyTitle = "Captain",
            DutyStartDate = Today.AddDays(1)
        }, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<BadHttpRequestException>();
        ex.Which.Message.Should().Contain("future");
    }

    [Fact]
    public async Task Handle_WhenFirstDutyIsRetired_UsesLowestRank()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        TestContextFactory.SeedRanks(context);
        var person = await TestContextFactory.SeedPersonAsync(context, "RetiredFirst");
        var handler = new CreateAstronautDutyHandler(context);

        var result = await handler.Handle(new CreateAstronautDuty
        {
            Name = "RetiredFirst",
            RankId = 5,
            DutyTitle = "RETIRED",
            DutyStartDate = Today
        }, CancellationToken.None);

        var duty = await context.AstronautDuties.FindAsync(result.Id);
        duty.Should().NotBeNull();
        duty!.RankId.Should().Be(1);
        duty.DutyTitle.Should().Be("RETIRED");
    }

    [Fact]
    public async Task Handle_WhenNoCurrentDuty_CreatesDutyAndAstronautDetail()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        TestContextFactory.SeedRanks(context);
        var person = await TestContextFactory.SeedPersonAsync(context, "FirstDuty");
        var handler = new CreateAstronautDutyHandler(context);

        var result = await handler.Handle(new CreateAstronautDuty
        {
            Name = "FirstDuty",
            RankId = 2,
            DutyTitle = "Sergeant",
            DutyStartDate = Yesterday
        }, CancellationToken.None);

        result.Id.Should().BeGreaterThan(0);
        var duty = await context.AstronautDuties.FindAsync(result.Id);
        duty.Should().NotBeNull();
        duty!.PersonId.Should().Be(person.Id);
        duty.RankId.Should().Be(2);
        duty.DutyTitle.Should().Be("Sergeant");
        duty.DutyEndDate.Should().BeNull();

        var detail = await context.AstronautDetails.FirstOrDefaultAsync(a => a.PersonId == person.Id);
        detail.Should().NotBeNull();
        detail!.CareerStartDate.Should().Be(Yesterday.Date);
        detail.CareerEndDate.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenCurrentDutyExists_SetsPreviousDutyEndDate()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        TestContextFactory.SeedRanks(context);
        var person = await TestContextFactory.SeedPersonAsync(context, "TwoDuties");
        context.AstronautDetails.Add(new AstronautDetail { PersonId = person.Id, CareerStartDate = Yesterday });
        context.AstronautDuties.Add(new AstronautDuty
        {
            PersonId = person.Id,
            RankId = 1,
            DutyTitle = "First",
            DutyStartDate = Yesterday,
            DutyEndDate = null
        });
        await context.SaveChangesAsync();

        var handler = new CreateAstronautDutyHandler(context);
        await handler.Handle(new CreateAstronautDuty
        {
            Name = "TwoDuties",
            RankId = 2,
            DutyTitle = "Second",
            DutyStartDate = Today
        }, CancellationToken.None);

        var previousDuty = await context.AstronautDuties
            .FirstOrDefaultAsync(d => d.PersonId == person.Id && d.DutyTitle == "First");
        previousDuty.Should().NotBeNull();
        previousDuty!.DutyEndDate.Should().Be(Today.AddDays(-1).Date);
    }

    [Fact]
    public async Task Handle_WhenCurrentDutyStartedToday_ThrowsBadRequest()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        TestContextFactory.SeedRanks(context);
        var person = await TestContextFactory.SeedPersonAsync(context, "Today");
        context.AstronautDetails.Add(new AstronautDetail { PersonId = person.Id, CareerStartDate = Today });
        context.AstronautDuties.Add(new AstronautDuty
        {
            PersonId = person.Id,
            RankId = 1,
            DutyTitle = "Current",
            DutyStartDate = Today,
            DutyEndDate = null
        });
        await context.SaveChangesAsync();

        var handler = new CreateAstronautDutyHandler(context);
        var act = () => handler.Handle(new CreateAstronautDuty
        {
            Name = "Today",
            RankId = 2,
            DutyTitle = "Next",
            DutyStartDate = Today
        }, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<BadHttpRequestException>();
        ex.Which.Message.Should().Contain("tomorrow");
    }

    [Fact]
    public async Task Handle_WhenRetired_SetsCareerEndDate()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        TestContextFactory.SeedRanks(context);
        var person = await TestContextFactory.SeedPersonAsync(context, "Retire");
        context.AstronautDetails.Add(new AstronautDetail { PersonId = person.Id, CareerStartDate = Yesterday });
        context.AstronautDuties.Add(new AstronautDuty
        {
            PersonId = person.Id,
            RankId = 2,
            DutyTitle = "Active",
            DutyStartDate = Yesterday,
            DutyEndDate = null
        });
        await context.SaveChangesAsync();

        var handler = new CreateAstronautDutyHandler(context);
        await handler.Handle(new CreateAstronautDuty
        {
            Name = "Retire",
            RankId = 2,
            DutyTitle = "RETIRED",
            DutyStartDate = Today
        }, CancellationToken.None);

        var detail = await context.AstronautDetails.FirstAsync(a => a.PersonId == person.Id);
        detail.CareerEndDate.Should().NotBeNull("handler must set career end date when assigning RETIRED duty");
    }
}
