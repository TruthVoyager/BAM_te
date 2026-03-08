using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
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

    private static void SetControllerContext(ControllerBase controller)
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
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

    [Fact]
    public async Task CreateAstronautDuty_WhenPersonNotFound_Returns400()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        TestContextFactory.SeedRanks(context);
        var sp = BuildServiceProvider(context);
        var controller = sp.GetRequiredService<AstronautDutyController>();
        SetControllerContext(controller);

        var result = await controller.CreateAstronautDuty(new CreateAstronautDuty
        {
            Name = "NoSuchPerson",
            RankId = 1,
            DutyTitle = "Pilot",
            DutyStartDate = DateTime.Today
        });

        var obj = result.Should().BeOfType<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task CreateAstronautDuty_WhenDuplicateDuty_Returns400()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        TestContextFactory.SeedRanks(context);
        await TestContextFactory.SeedPersonAsync(context, "Astro");
        var sp = BuildServiceProvider(context);
        var controller = sp.GetRequiredService<AstronautDutyController>();
        SetControllerContext(controller);
        var request = new CreateAstronautDuty
        {
            Name = "Astro",
            RankId = 1,
            DutyTitle = "Pilot",
            DutyStartDate = DateTime.Today
        };

        await controller.CreateAstronautDuty(request);
        var result = await controller.CreateAstronautDuty(request);

        var obj = result.Should().BeOfType<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(400);
    }
}
