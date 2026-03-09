using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using StargateAPI.Business.Services;
using StargateAPI.Controllers;
using NUnit.Framework;

namespace StargateAPI.Tests.Controllers;

[TestFixture]
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
            cfg.AddRequestPreProcessor<CreatePersonPreProcessor>();
        });
        services.AddSingleton<IProcessLogService>(NullProcessLogService.Instance);
        services.AddSingleton<PersonController>();
        return services.BuildServiceProvider();
    }

    private static void SetControllerContext(ControllerBase controller)
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [Test]
    public async Task GetPeople_ReturnsOkWithList()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        var sp = BuildServiceProvider(context);
        var controller = sp.GetRequiredService<PersonController>();

        var result = await controller.GetPeople();

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var ok = (ObjectResult)result;
        Assert.That(ok.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetPersonByName_WhenExists_ReturnsOk()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        await TestContextFactory.SeedPersonAsync(context, "TestPerson");
        var sp = BuildServiceProvider(context);
        var controller = sp.GetRequiredService<PersonController>();

        var result = await controller.GetPersonByName("TestPerson");

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var ok = (ObjectResult)result;
        Assert.That(ok.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task CreatePerson_WhenValid_ReturnsOkWithId()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        var sp = BuildServiceProvider(context);
        var controller = sp.GetRequiredService<PersonController>();

        var result = await controller.CreatePerson("NewPerson");

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var ok = (ObjectResult)result;
        Assert.That(ok.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task UpdatePerson_WhenValid_ReturnsOk()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        await TestContextFactory.SeedPersonAsync(context, "Before");
        var sp = BuildServiceProvider(context);
        var controller = sp.GetRequiredService<PersonController>();

        var result = await controller.UpdatePerson("Before", "After");

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var ok = (ObjectResult)result;
        Assert.That(ok.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task CreatePerson_WhenDuplicateName_Returns409()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        await TestContextFactory.SeedPersonAsync(context, "Existing");
        var sp = BuildServiceProvider(context);
        var controller = sp.GetRequiredService<PersonController>();
        SetControllerContext(controller);

        var result = await controller.CreatePerson("Existing");

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var obj = (ObjectResult)result;
        Assert.That(obj.StatusCode, Is.EqualTo(409));
    }

    [Test]
    public async Task UpdatePerson_WhenPersonNotFound_Returns404()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        var sp = BuildServiceProvider(context);
        var controller = sp.GetRequiredService<PersonController>();
        SetControllerContext(controller);

        var result = await controller.UpdatePerson("NoSuchPerson", "NewName");

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var obj = (ObjectResult)result;
        Assert.That(obj.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task UpdatePerson_WhenNewNameAlreadyExists_Returns409()
    {
        var context = TestContextFactory.CreateInMemoryContext();
        await TestContextFactory.SeedPersonAsync(context, "PersonA");
        await TestContextFactory.SeedPersonAsync(context, "PersonB");
        var sp = BuildServiceProvider(context);
        var controller = sp.GetRequiredService<PersonController>();
        SetControllerContext(controller);

        var result = await controller.UpdatePerson("PersonA", "PersonB");

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var obj = (ObjectResult)result;
        Assert.That(obj.StatusCode, Is.EqualTo(409));
    }
}
