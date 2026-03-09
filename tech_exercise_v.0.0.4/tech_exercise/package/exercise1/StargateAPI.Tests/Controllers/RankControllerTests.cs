using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;
using NUnit.Framework;

namespace StargateAPI.Tests.Controllers;

[TestFixture]
public class RankControllerTests
{
    [Test]
    public async Task GetRanks_WhenRanksExist_ReturnsOkWithOrderedRanks()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        TestContextFactory.SeedRanks(context);
        var controller = new RankController(context, NullLogger<RankController>.Instance, NullProcessLogService.Instance);

        var result = await controller.GetRanks();

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        Assert.That(okResult.Value, Is.InstanceOf<List<Rank>>());
        var ranks = (List<Rank>)okResult.Value!;
        Assert.That(ranks, Has.Count.EqualTo(5));
        Assert.That(ranks.Select(r => r.Level).ToList(), Is.Ordered.Ascending);
    }

    [Test]
    public async Task GetRanks_WhenNoRanks_ReturnsOkWithEmptyList()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        var controller = new RankController(context, NullLogger<RankController>.Instance, NullProcessLogService.Instance);

        var result = await controller.GetRanks();

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        Assert.That(okResult.Value, Is.InstanceOf<List<Rank>>());
        var ranks = (List<Rank>)okResult.Value!;
        Assert.That(ranks, Is.Empty);
    }
}
