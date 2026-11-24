using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Kitpymes.Core.Results;
using Kitpymes.Core.Validations.FluentValidation.Adapter;
using Kitpymes.Core.Validations.FluentValidation.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KValidator = Kitpymes.Core.Validations.Abstractions.IValidator<Kitpymes.Core.Validations.FluentValidation.Tests.Adapter.Customer>;

namespace Kitpymes.Core.Validations.FluentValidation.Tests.Adapter;

public sealed class Street
{
    public string? Name { get; set; }
}

public sealed class Item
{
    public string? Code { get; set; }
}

public sealed class Customer
{
    public string? Name { get; set; }

    public string? Email { get; set; }

    public int Age { get; set; }

    public Street? Address { get; set; }

    public List<Item> Items { get; set; } = new ();

    public string? Other { get; set; }
}

public sealed class CustomerValidator : AbstractValidator<Customer>
{
    public CustomerValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Email).EmailAddress();
        RuleFor(x => x.Age).InclusiveBetween(18, 99);
        RuleFor(x => x.Address!.Name).MaximumLength(3).When(x => x.Address is not null);
        RuleForEach(x => x.Items).ChildRules(item => item.RuleFor(i => i.Code).NotNull());
        RuleFor(x => x.Other).Must(o => o != "propio").WithErrorCode("Clientes.Propio");
        RuleFor(x => x.Other).Must(o => o != "malo").WithErrorCode("codigo malo");
        RuleFor(x => x.Other).Must(o => o != "reservado").WithErrorCode("Validations.Reservado");
        RuleFor(x => x.Name).Custom((name, context) =>
        {
            if (name == "raiz")
            {
                context.AddFailure(new ValidationFailure(string.Empty, "El cliente no es válido."));
            }
        });
    }
}

[TestClass]
public class AdapterTests
{
    private static Customer Good() => new () { Name = "Ana", Email = "ana@kitpymes.com", Age = 30 };

    private static async Task<Result> Validate(Customer customer)
        => await new FluentValidationAdapter<Customer>(new[] { new CustomerValidator() }).ValidateAsync(customer);

    private static async Task<FieldError> Single(Customer customer)
    {
        var result = await Validate(customer);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(ValidationErrorCodes.Invalid, result.Error.Code);
        Assert.AreEqual(ErrorType.Validation, result.Error.Type);
        Assert.AreEqual(1, result.Error.Fields.Count, string.Join(" | ", result.Error.Fields));

        return result.Error.Fields[0];
    }

    [TestMethod]
    public async Task Valid_Returns_Success()
        => Assert.IsTrue((await Validate(Good())).IsSuccess);

    [TestMethod]
    public async Task Built_In_Rules_Get_The_Same_Codes_As_Validator()
    {
        var name = Good();
        name.Name = string.Empty;
        var error = await Single(name);
        Assert.AreEqual("name", error.Field);
        Assert.AreEqual(ValidationErrorCodes.Required, error.Code);
        Assert.IsFalse(string.IsNullOrWhiteSpace(error.Message));

        var email = Good();
        email.Email = "ana";
        Assert.AreEqual(ValidationErrorCodes.Email, (await Single(email)).Code);

        var age = Good();
        age.Age = 5;
        Assert.AreEqual(ValidationErrorCodes.Between, (await Single(age)).Code);
    }

    [TestMethod]
    public async Task Paths_Use_The_Angular_Format()
    {
        var nested = Good();
        nested.Address = new Street { Name = "Larga" };
        var error = await Single(nested);
        Assert.AreEqual("address.name", error.Field);
        Assert.AreEqual(ValidationErrorCodes.MaxLength, error.Code);

        var items = Good();
        items.Items.Add(new Item { Code = "a" });
        items.Items.Add(new Item());
        error = await Single(items);
        Assert.AreEqual("items.1.code", error.Field);
        Assert.AreEqual(ValidationErrorCodes.Required, error.Code);
    }

    [TestMethod]
    public async Task Own_Codes_Are_Kept_Only_If_Valid_And_Not_Reserved()
    {
        foreach (var (other, code) in new[] { ("propio", "Clientes.Propio"), ("malo", ValidationErrorCodes.Condition), ("reservado", ValidationErrorCodes.Condition) })
        {
            var customer = Good();
            customer.Other = other;
            var error = await Single(customer);
            Assert.AreEqual("other", error.Field);
            Assert.AreEqual(code, error.Code);
        }
    }

    [TestMethod]
    public async Task Failures_Without_Field_Go_To_The_Message()
    {
        var customer = Good();
        customer.Name = "raiz";

        var result = await Validate(customer);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(0, result.Error.Fields.Count);
        StringAssert.Contains(result.Error.Message, "El cliente no es válido.");
    }

    [TestMethod]
    public async Task Null_Instance_Fails_Without_Fields()
    {
        var result = await Validate(null!);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(ValidationErrorCodes.Invalid, result.Error.Code);
        Assert.AreEqual(0, result.Error.Fields.Count);
    }

    [TestMethod]
    public async Task Message_Follows_The_Culture()
    {
        var culture = CultureInfo.CurrentUICulture;

        try
        {
            var customer = Good();
            customer.Name = null;

            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
            Assert.AreEqual("There are validation errors.", (await Validate(customer)).Error!.Message);

            CultureInfo.CurrentUICulture = new CultureInfo("es-ES");
            Assert.AreEqual("Hay errores de validación.", (await Validate(customer)).Error!.Message);
        }
        finally
        {
            CultureInfo.CurrentUICulture = culture;
        }
    }

    [TestMethod]
    public void Needs_Exactly_One_FluentValidation_Validator()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new FluentValidationAdapter<Customer>(null!));
        Assert.ThrowsExactly<InvalidOperationException>(() => new FluentValidationAdapter<Customer>(Array.Empty<IValidator<Customer>>()));
        Assert.ThrowsExactly<InvalidOperationException>(() => new FluentValidationAdapter<Customer>(new IValidator<Customer>[] { new CustomerValidator(), new CustomerValidator() }));
    }

    [TestMethod]
    public async Task LoadFluentValidationAdapters_Registers_The_Adapter_Once()
    {
        var services = new ServiceCollection();
        services.LoadFluentValidationAdapters(typeof(AdapterTests).Assembly);
        services.LoadFluentValidationAdapters(typeof(AdapterTests).Assembly);

        Assert.AreEqual(1, services.Count(d => d.ServiceType == typeof(KValidator)));
        Assert.AreEqual(1, services.Count(d => d.ServiceType == typeof(IValidator<Customer>)));
        Assert.AreEqual(ServiceLifetime.Scoped, services.Single(d => d.ServiceType == typeof(KValidator)).Lifetime);

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var adapter = scope.ServiceProvider.GetRequiredService<KValidator>();

        Assert.IsInstanceOfType<FluentValidationAdapter<Customer>>(adapter);
        Assert.IsTrue((await adapter.ValidateAsync(Good())).IsSuccess);
    }

    [TestMethod]
    public void LoadFluentValidationAdapters_Rejects_Empty_And_Null()
    {
        var services = new ServiceCollection();

        Assert.ThrowsExactly<ArgumentException>(() => services.LoadFluentValidationAdapters());
        Assert.ThrowsExactly<ArgumentException>(() => services.LoadFluentValidationAdapters(null!, typeof(AdapterTests).Assembly));
        Assert.ThrowsExactly<ArgumentNullException>(() => services.LoadFluentValidationAdapters(null!));
    }
}
