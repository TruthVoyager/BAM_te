using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using StargateAPI.Business.Services;
using StargateAPI.Controllers;
using Xunit;

namespace StargateAPI.Tests.Controllers;

public class PersonControllerTests
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
        services.AddSingleton<PersonController>();
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task GetPeople_ReturnsOkWithList()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        var sp = BuildServiceProvider(context);
        var controller = sp.GetRequiredService<PersonController>();

        var result = await controller.GetPeople();

        var ok = result.Should().BeOfType<ObjectResult>().Subject;
        ok.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetPersonByName_WhenExists_ReturnsOk()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        await TestContextFactory.SeedPersonAsync(context, "TestPerson");
        var sp = BuildServiceProvider(context);
        var controller = sp.GetRequiredService<PersonController>();

        var result = await controller.GetPersonByName("TestPerson");

        var ok = result.Should().BeOfType<ObjectResult>().Subject;
        ok.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task CreatePerson_WhenValid_ReturnsOkWithId()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        var sp = BuildServiceProvider(context);
        var controller = sp.GetRequiredService<PersonController>();

        var result = await controller.CreatePerson("NewPerson");

        var ok = result.Should().BeOfType<ObjectResult>().Subject;
        ok.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task UpdatePerson_WhenValid_ReturnsOk()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        await TestContextFactory.SeedPersonAsync(context, "Before");
        var sp = BuildServiceProvider(context);
        var controller = sp.GetRequiredService<PersonController>();

        var result = await controller.UpdatePerson("Before", "After");

        var ok = result.Should().BeOfType<ObjectResult>().Subject;
        ok.StatusCode.Should().Be(200);
    }
}
