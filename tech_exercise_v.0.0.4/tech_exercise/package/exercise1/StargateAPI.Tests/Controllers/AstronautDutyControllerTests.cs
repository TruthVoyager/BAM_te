using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using StargateAPI.Business.Services;
using StargateAPI.Controllers;
using Xunit;

namespace StargateAPI.Tests.Controllers;

public class AstronautDutyControllerTests
{
    private static IServiceProvider BuildServiceProvider(StargateContext context)
    {
        var services = new ServiceCollection();
        services.AddSingleton(context);
        services.AddLogging();
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CreatePerson).Assembly);
            cfg.AddRequestPreProcessor<CreateAstronautDutyPreProcessor>();
        });
        services.AddSingleton<IProcessLogService>(NullProcessLogService.Instance);
        services.AddSingleton<AstronautDutyController>();
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task GetAstronautDutiesByName_ReturnsOk()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        await TestContextFactory.SeedPersonAsync(context, "DutyPerson");
        var sp = BuildServiceProvider(context);
        var controller = sp.GetRequiredService<AstronautDutyController>();

        var result = await controller.GetAstronautDutiesByName("DutyPerson");

        var ok = result.Should().BeOfType<ObjectResult>().Subject;
        ok.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task CreateAstronautDuty_WhenValid_ReturnsOk()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        TestContextFactory.SeedRanks(context);
        await TestContextFactory.SeedPersonAsync(context, "Astro");
        var sp = BuildServiceProvider(context);
        var controller = sp.GetRequiredService<AstronautDutyController>();

        var result = await controller.CreateAstronautDuty(new CreateAstronautDuty
        {
            Name = "Astro",
            RankId = 1,
            DutyTitle = "Pilot",
            DutyStartDate = DateTime.Today
        });

        var ok = result.Should().BeOfType<ObjectResult>().Subject;
        ok.StatusCode.Should().Be(200);
    }
}
