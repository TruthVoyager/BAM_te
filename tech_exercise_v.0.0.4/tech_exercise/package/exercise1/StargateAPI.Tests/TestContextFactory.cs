using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;

namespace StargateAPI.Tests;

/// <summary>
/// Builds an in-memory StargateContext with optional seed data for unit tests.
/// </summary>
public static class TestContextFactory
{
    public static StargateContext CreateInMemoryContext(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<StargateContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;

        var context = new StargateContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    /// <summary>
    /// Seeds the Rank table (required for duty creation and queries that join to Rank).
    /// </summary>
    public static void SeedRanks(StargateContext context)
    {
        if (context.Ranks.Any()) return;

        context.Ranks.AddRange(
            new Rank { Id = 1, Name = "Specialist", Level = 1 },
            new Rank { Id = 2, Name = "Sergeant", Level = 2 },
            new Rank { Id = 3, Name = "Captain", Level = 3 },
            new Rank { Id = 4, Name = "Colonel", Level = 4 },
            new Rank { Id = 5, Name = "General", Level = 5 }
        );
        context.SaveChanges();
    }

    public static async Task<Person> SeedPersonAsync(StargateContext context, string name)
    {
        var person = new Person { Name = name };
        context.People.Add(person);
        await context.SaveChangesAsync();
        return person;
    }
}
