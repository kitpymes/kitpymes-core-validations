using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kitpymes.Core.Mediator.Abstractions;
using Kitpymes.Core.Results;
using Kitpymes.Core.Validations.Abstractions;
using Kitpymes.Core.Validations.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kitpymes.Core.Validations.Tests.Pipeline;

[TestClass]
public class PipelineTests
{
    private static ServiceCollection Services()
    {
        var services = new ServiceCollection();
        services.LoadValidations(o => o.WithAssemblies(typeof(PipelineTests).Assembly));
        return services;
    }

    private static Task<Result> Check(IServiceCollection services, params Type[] requests)
    {
        using var provider = services.BuildServiceProvider();
        var check = provider.GetRequiredService<IValidationStartupCheck>();
        return check.CheckAsync(new Catalog(requests), provider, CancellationToken.None);
    }

    [TestMethod]
    public void Registers_Validators_As_Singletons_Once()
    {
        var services = Services();
        services.LoadValidations(o => o.WithAssemblies(typeof(PipelineTests).Assembly, typeof(PipelineTests).Assembly));

        var validators = services.Where(d => d.ServiceType == typeof(IValidator<CreateOrder>)).ToList();

        Assert.AreEqual(1, validators.Count);
        Assert.AreEqual(ServiceLifetime.Singleton, validators[0].Lifetime);
        Assert.AreEqual(typeof(CreateOrderValidator), validators[0].ImplementationType);
        Assert.AreEqual(1, services.Count(d => d.ServiceType == typeof(IPipelineBehavior<,>)));
        Assert.AreEqual(ServiceLifetime.Transient, services.Single(d => d.ServiceType == typeof(IPipelineBehavior<,>)).Lifetime);
        Assert.AreEqual(1, services.Count(d => d.ServiceType == typeof(IValidationStartupCheck)));
        Assert.AreEqual(2, services.Count(d => d.ServiceType == typeof(IValidator<Twice>)));
        Assert.AreEqual(1, services.Count(d => d.ServiceType == typeof(IValidator<ManualRequest>)));
    }

    [TestMethod]
    public void Without_Assemblies_Registers_Nothing_New()
    {
        var services = new ServiceCollection();
        services.LoadValidations(_ => { });

        Assert.AreEqual(0, services.Count);
    }

    [TestMethod]
    public void WithAssemblies_Rejects_Empty_And_Null()
    {
        var services = new ServiceCollection();

        Assert.ThrowsExactly<ArgumentException>(() => services.LoadValidations(o => o.WithAssemblies()));
        Assert.ThrowsExactly<ArgumentException>(() => services.LoadValidations(o => o.WithAssemblies(null!, typeof(PipelineTests).Assembly)));
        Assert.ThrowsExactly<ArgumentNullException>(() => services.LoadValidations(o => o.WithAssemblies(null!)));
        Assert.ThrowsExactly<ArgumentNullException>(() => services.LoadValidations((Action<Settings.ValidationsOptions>)null!));
    }

    [TestMethod]
    public async Task Behavior_Calls_The_Handler_When_Valid()
    {
        using var provider = Services().BuildServiceProvider();
        var behavior = provider.GetServices<IPipelineBehavior<CreateOrder, Result<int>>>().Single();
        var calls = 0;

        var result = await behavior.HandleAsync(new CreateOrder { Name = "a" }, () => { calls++; return Task.FromResult(Result.Success(7)); }, CancellationToken.None);

        Assert.AreEqual(1, calls);
        Assert.AreEqual(7, result.Value);
    }

    [TestMethod]
    public async Task Behavior_Returns_The_Validation_Error_Without_Calling_The_Handler()
    {
        using var provider = Services().BuildServiceProvider();
        var behavior = provider.GetServices<IPipelineBehavior<CreateOrder, Result<int>>>().Single();
        var calls = 0;

        var result = await behavior.HandleAsync(new CreateOrder(), () => { calls++; return Task.FromResult(Result.Success(7)); }, CancellationToken.None);

        Assert.AreEqual(0, calls);
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(ErrorType.Validation, result.Error.Type);
        Assert.AreEqual("name", result.Error.Fields.Single().Field);
    }

    [TestMethod]
    public async Task Behavior_Without_Validator_Calls_The_Handler()
    {
        using var provider = Services().BuildServiceProvider();
        var behavior = provider.GetServices<IPipelineBehavior<WithoutValidator, Result>>().Single();

        var result = await behavior.HandleAsync(new WithoutValidator(), () => Task.FromResult(Result.Success()), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public async Task Behavior_With_Two_Validators_Or_A_Null_Result_Throws()
    {
        using var provider = Services().BuildServiceProvider();
        var twice = provider.GetServices<IPipelineBehavior<Twice, Result>>().Single();
        var manual = provider.GetServices<IPipelineBehavior<ManualRequest, Result>>().Single();

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => twice.HandleAsync(new Twice(), () => Task.FromResult(Result.Success()), CancellationToken.None));
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => manual.HandleAsync(new ManualRequest { Value = "null" }, () => Task.FromResult(Result.Success()), CancellationToken.None));

        var ok = await manual.HandleAsync(new ManualRequest(), () => Task.FromResult(Result.Success()), CancellationToken.None);
        Assert.IsTrue(ok.IsSuccess);
    }

    [TestMethod]
    public async Task StartupCheck_Passes_With_A_Validator_Each_Or_Without_Input()
    {
        var result = await Check(Services(), typeof(CreateOrder), typeof(Ping), typeof(ManualRequest));

        Assert.IsTrue(result.IsSuccess, result.Error?.Message ?? string.Empty);
    }

    [TestMethod]
    public async Task StartupCheck_Lists_Every_Problem()
    {
        var result = await Check(Services(), typeof(WithoutValidator), typeof(WithField), typeof(Twice), typeof(BadRoot), typeof(CreateOrder));

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(ErrorType.Unexpected, result.Error.Type);
        Assert.AreEqual(ValidationErrorCodes.Configuration, result.Error.Code);

        var message = result.Error.Message;
        StringAssert.Contains(message, typeof(WithoutValidator).FullName + " no tiene validador");
        StringAssert.Contains(message, typeof(WithField).FullName + " no tiene validador");
        StringAssert.Contains(message, typeof(Twice).FullName + " tiene 2 validadores");
        StringAssert.Contains(message, "BadRootValidator: Una regla sobre el objeto entero de BadRoot necesita WithName");
        Assert.IsFalse(message.Contains(typeof(CreateOrder).FullName!, StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task StartupCheck_Fails_If_The_Behavior_Is_Not_Registered()
    {
        var services = Services();
        services.Remove(services.Single(d => d.ServiceType == typeof(IPipelineBehavior<,>)));

        var result = await Check(services, typeof(CreateOrder), typeof(Ping));

        Assert.IsTrue(result.IsFailure);
        StringAssert.Contains(result.Error.Message, "El comportamiento de validación no está registrado para " + typeof(CreateOrder).FullName);
        Assert.IsFalse(result.Error.Message.Contains(typeof(Ping).FullName!, StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task StartupCheck_Honors_Cancellation()
    {
        using var source = new CancellationTokenSource();
        await source.CancelAsync();
        using var provider = Services().BuildServiceProvider();
        var check = provider.GetRequiredService<IValidationStartupCheck>();

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => check.CheckAsync(new Catalog(typeof(CreateOrder)), provider, source.Token));
    }
}
