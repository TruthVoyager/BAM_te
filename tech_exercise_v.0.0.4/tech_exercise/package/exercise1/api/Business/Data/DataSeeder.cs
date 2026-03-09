using Microsoft.EntityFrameworkCore;

namespace StargateAPI.Business.Data;

public static class DataSeeder
{
    public static async Task SeedIfEmptyAsync(StargateContext context, CancellationToken cancellationToken = default)
    {
        if (await context.People.AnyAsync(cancellationToken))
            return;

        var todayUtc = DateTime.UtcNow.Date;
        var yesterday = todayUtc.AddDays(-1);

        var person1 = new Person { Name = "Alex Vance" };
        context.People.Add(person1);
        await context.SaveChangesAsync(cancellationToken);

        var detail1 = new AstronautDetail
        {
            PersonId = person1.Id,
            CareerStartDate = todayUtc.AddDays(-30)
        };
        context.AstronautDetails.Add(detail1);

        context.AstronautDuties.AddRange(
            new AstronautDuty
            {
                PersonId = person1.Id,
                RankId = 1,
                DutyTitle = "Trainee",
                DutyStartDate = todayUtc.AddDays(-30),
                DutyEndDate = todayUtc.AddDays(-21)
            },
            new AstronautDuty
            {
                PersonId = person1.Id,
                RankId = 2,
                DutyTitle = "Sergeant",
                DutyStartDate = todayUtc.AddDays(-20),
                DutyEndDate = todayUtc.AddDays(-11)
            },
            new AstronautDuty
            {
                PersonId = person1.Id,
                RankId = 3,
                DutyTitle = "Captain",
                DutyStartDate = todayUtc.AddDays(-10),
                DutyEndDate = todayUtc.AddDays(-2)
            },
            new AstronautDuty
            {
                PersonId = person1.Id,
                RankId = 4,
                DutyTitle = "Mission Commander",
                DutyStartDate = yesterday,
                DutyEndDate = null
            }
        );

        var person2 = new Person { Name = "Sam Carter" };
        context.People.Add(person2);
        await context.SaveChangesAsync(cancellationToken);

        var retiredStart = todayUtc.AddDays(-10);
        var detail2 = new AstronautDetail
        {
            PersonId = person2.Id,
            CareerStartDate = todayUtc.AddDays(-20),
            CareerEndDate = retiredStart.AddDays(-1)
        };
        context.AstronautDetails.Add(detail2);

        context.AstronautDuties.AddRange(
            new AstronautDuty
            {
                PersonId = person2.Id,
                RankId = 2,
                DutyTitle = "Sergeant",
                DutyStartDate = todayUtc.AddDays(-20),
                DutyEndDate = todayUtc.AddDays(-11)
            },
            new AstronautDuty
            {
                PersonId = person2.Id,
                RankId = 3,
                DutyTitle = "RETIRED",
                DutyStartDate = retiredStart,
                DutyEndDate = null
            }
        );

        var person3 = new Person { Name = "Jordan Blake" };
        context.People.Add(person3);

        var person4 = new Person { Name = "Riley Chen" };
        context.People.Add(person4);
        await context.SaveChangesAsync(cancellationToken);

        var detail4 = new AstronautDetail
        {
            PersonId = person4.Id,
            CareerStartDate = todayUtc.AddDays(-5)
        };
        context.AstronautDetails.Add(detail4);
        context.AstronautDuties.Add(new AstronautDuty
        {
            PersonId = person4.Id,
            RankId = 2,
            DutyTitle = "Sergeant",
            DutyStartDate = todayUtc.AddDays(-5),
            DutyEndDate = null
        });

        await context.SaveChangesAsync(cancellationToken);
    }
}
