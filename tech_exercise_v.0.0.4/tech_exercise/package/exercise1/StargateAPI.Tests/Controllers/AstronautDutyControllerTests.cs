using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using StargateAPI.Business.Services;
using StargateAPI.Controllers;
using NUnit.Framework;

namespace StargateAPI.Tests.Controllers;

[TestFixture]
public class AstronautDutyControllerTests
{
    private static IServiceProvider BuildServiceProvider(StargateContext context, Mock<IProcessLogService>? processLogMock = null)
    {
        var processLog = processLogMock ?? CreateMockProcessLogService();
        var logger = new Mock<ILogger<AstronautDutyController>>();

        var services = new ServiceCollection();
        services.AddSingleton(context);
        services.AddLogging();
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CreatePerson).Assembly);
            cfg.AddRequestPreProcessor<CreateAstronautDutyPreProcessor>();
        });
        services.AddSingleton<IProcessLogService>(processLog.Object);
        services.AddSingleton<ILogger<AstronautDutyController>>(logger.Object);
        services.AddSingleton<AstronautDutyController>();
        return services.BuildServiceProvider();
    }

    private static Mock<IProcessLogService> CreateMockProcessLogService()
    {
        var mock = new Mock<IProcessLogService>();
        mock
            .Setup(s => s.LogExceptionAsync(It.IsAny<Exception>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return mock;
    }

    private static void SetControllerContext(ControllerBase controller)
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [Test]
    public async Task GetAstronautDutiesByName_ReturnsOk()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        await TestContextFactory.SeedPersonAsync(context, "DutyPerson");
        var sp = BuildServiceProvider(context);
        var controller = sp.GetRequiredService<AstronautDutyController>();

        var result = await controller.GetAstronautDutiesByName("DutyPerson");

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var ok = (ObjectResult)result;
        Assert.That(ok.StatusCode, Is.EqualTo(200));
    }

    [Test]
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

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var ok = (ObjectResult)result;
        Assert.That(ok.StatusCode, Is.EqualTo(200));
    }

    [Test]
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

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var obj = (ObjectResult)result;
        Assert.That(obj.StatusCode, Is.EqualTo(400));
    }

    [Test]
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

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var obj = (ObjectResult)result;
        Assert.That(obj.StatusCode, Is.EqualTo(400));
    }
}
