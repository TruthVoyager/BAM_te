using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;
using Xunit;

namespace StargateAPI.Tests.Controllers;

public class RankControllerTests
{
    [Fact]
    public async Task GetRanks_WhenRanksExist_ReturnsOkWithOrderedRanks()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        TestContextFactory.SeedRanks(context);
        var controller = new RankController(context, NullLogger<RankController>.Instance, NullProcessLogService.Instance);

        var result = await controller.GetRanks();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var ranks = okResult.Value.Should().BeAssignableTo<List<Rank>>().Subject;
        ranks.Should().HaveCount(5);
        ranks.Select(r => r.Level).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GetRanks_WhenNoRanks_ReturnsOkWithEmptyList()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        var controller = new RankController(context, NullLogger<RankController>.Instance, NullProcessLogService.Instance);

        var result = await controller.GetRanks();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var ranks = okResult.Value.Should().BeAssignableTo<List<Rank>>().Subject;
        ranks.Should().BeEmpty();
    }
}
