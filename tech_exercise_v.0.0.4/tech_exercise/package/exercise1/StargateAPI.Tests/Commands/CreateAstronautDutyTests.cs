using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using NUnit.Framework;

namespace StargateAPI.Tests.Commands;

[TestFixture]
public class CreateAstronautDutyTests
{
    private static readonly DateTime Today = DateTime.Today;
    private static readonly DateTime Yesterday = Today.AddDays(-1);

    [Test]
    public void PreProcessor_WhenPersonNotFound_ThrowsBadRequest()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        TestContextFactory.SeedRanks(context);
        var preProcessor = new CreateAstronautDutyPreProcessor(context);

        Assert.ThrowsAsync<BadHttpRequestException>(async () => await preProcessor.Process(new CreateAstronautDuty
        {
            Name = "NoSuchPerson",
            RankId = 1,
            DutyTitle = "Captain",
            DutyStartDate = Today
        }, CancellationToken.None));
    }

    [Test]
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
        var ex = Assert.ThrowsAsync<BadHttpRequestException>(async () => await preProcessor.Process(new CreateAstronautDuty
        {
            Name = "Dup",
            RankId = 1,
            DutyTitle = "Captain",
            DutyStartDate = Today
        }, CancellationToken.None));
        Assert.That(ex!.Message, Does.Contain("already has a duty with this title and start date"));
    }

    [Test]
    public void Handle_WhenPersonNotFound_Throws()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        TestContextFactory.SeedRanks(context);
        var handler = new CreateAstronautDutyHandler(context);

        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await handler.Handle(new CreateAstronautDuty
        {
            Name = "NoOne",
            RankId = 1,
            DutyTitle = "Captain",
            DutyStartDate = Today
        }, CancellationToken.None));
        Assert.That(ex!.Message, Is.EqualTo("Person not found."));
    }

    [Test]
    public async Task Handle_WhenStartDateInFuture_ThrowsBadRequest()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        TestContextFactory.SeedRanks(context);
        await TestContextFactory.SeedPersonAsync(context, "Future");
        var handler = new CreateAstronautDutyHandler(context);

        var ex = Assert.ThrowsAsync<BadHttpRequestException>(async () => await handler.Handle(new CreateAstronautDuty
        {
            Name = "Future",
            RankId = 1,
            DutyTitle = "Captain",
            DutyStartDate = Today.AddDays(1)
        }, CancellationToken.None));
        Assert.That(ex!.Message, Does.Contain("future"));
    }

    [Test]
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
        Assert.That(duty, Is.Not.Null);
        Assert.That(duty!.RankId, Is.EqualTo(1));
        Assert.That(duty.DutyTitle, Is.EqualTo("RETIRED"));
    }

    [Test]
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

        Assert.That(result.Id, Is.GreaterThan(0));
        var duty = await context.AstronautDuties.FindAsync(result.Id);
        Assert.That(duty, Is.Not.Null);
        Assert.That(duty!.PersonId, Is.EqualTo(person.Id));
        Assert.That(duty.RankId, Is.EqualTo(2));
        Assert.That(duty.DutyTitle, Is.EqualTo("Sergeant"));
        Assert.That(duty.DutyEndDate, Is.Null);

        var detail = await context.AstronautDetails.FirstOrDefaultAsync(a => a.PersonId == person.Id);
        Assert.That(detail, Is.Not.Null);
        Assert.That(detail!.CareerStartDate, Is.EqualTo(Yesterday.Date));
        Assert.That(detail.CareerEndDate, Is.Null);
    }

    [Test]
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
        Assert.That(previousDuty, Is.Not.Null);
        Assert.That(previousDuty!.DutyEndDate, Is.EqualTo(Today.AddDays(-1).Date));
    }

    [Test]
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
        var ex = Assert.ThrowsAsync<BadHttpRequestException>(async () => await handler.Handle(new CreateAstronautDuty
        {
            Name = "Today",
            RankId = 2,
            DutyTitle = "Next",
            DutyStartDate = Today
        }, CancellationToken.None));
        Assert.That(ex!.Message, Does.Contain("tomorrow"));
    }

    [Test]
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
        Assert.That(detail.CareerEndDate, Is.Not.Null, "handler must set career end date when assigning RETIRED duty");
    }
}
