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
    [System.Text.Json.Serialization.JsonPropertyName("nombre_calle")]
    public string? Label { get; set; }

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

public enum Kind
{
    A = 0,
}

public sealed class Sample
{
    [System.Text.Json.Serialization.JsonPropertyName("texto_libre")]
    public string? Renamed { get; set; }

    public List<Street> Streets { get; set; } = new ();

    public Dictionary<string, Street> ByKey { get; set; } = new ();

    public string? Text { get; set; }

    public int Number { get; set; }

    public Kind Kind { get; set; }
}

/// <summary>No se registra: es abstracto.</summary>
public abstract class BaseItemValidator : AbstractValidator<Item>
{
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
        Assert.ThrowsExactly<InvalidOperationException>(() => new FluentValidationAdapter<Customer>(new IValidator<Customer>[] { new CustomerValidator(), new InlineValidator<Customer>() }));
    }

    [TestMethod]
    public async Task The_Same_Validator_Registered_Twice_Counts_As_One()
    {
        var adapter = new FluentValidationAdapter<Customer>(new IValidator<Customer>[] { new CustomerValidator(), new CustomerValidator() });

        Assert.IsTrue((await adapter.ValidateAsync(Good())).IsSuccess);
    }

    [TestMethod]
    public async Task Paths_Respect_JsonPropertyName_Also_Inside_Collections()
    {
        Assert.AreEqual("texto_libre", (await One(v => v.RuleFor(x => x.Renamed).NotNull(), new Sample())).Field);

        var sample = new Sample();
        sample.Streets.Add(new Street());
        Assert.AreEqual("streets.0.nombre_calle", (await One(v => v.RuleForEach(x => x.Streets).ChildRules(s => s.RuleFor(x => x.Label).NotNull()), sample)).Field);

        Assert.AreEqual("byKey.casa.nombre_calle", (await One(v => v.RuleFor(x => x.Text).NotNull().OverridePropertyName("ByKey[casa].Label"), new Sample { Text = null })).Field);
        Assert.AreEqual("desconocido.label", (await One(v => v.RuleFor(x => x.Text).NotNull().OverridePropertyName("Desconocido.Label"), new Sample())).Field);
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

    private static async Task<FieldError> One(Action<InlineValidator<Sample>> rules, Sample sample)
    {
        var validator = new InlineValidator<Sample>();
        rules(validator);

        var result = await new FluentValidationAdapter<Sample>(new[] { validator }).ValidateAsync(sample);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(1, result.Error.Fields.Count, string.Join(" | ", result.Error.Fields));

        return result.Error.Fields[0];
    }

    [TestMethod]
    public async Task Every_Mapped_Rule_Gets_Its_Code()
    {
        var cases = new (Action<InlineValidator<Sample>> Rules, Sample Sample, string Code)[]
        {
            (v => v.RuleFor(x => x.Text).NotNull(), new Sample(), ValidationErrorCodes.Required),
            (v => v.RuleFor(x => x.Text).MinimumLength(3), new Sample { Text = "a" }, ValidationErrorCodes.MinLength),
            (v => v.RuleFor(x => x.Text).Matches("^x$"), new Sample { Text = "a" }, ValidationErrorCodes.Format),
            (v => v.RuleFor(x => x.Number).GreaterThan(0), new Sample(), ValidationErrorCodes.GreaterThan),
            (v => v.RuleFor(x => x.Number).GreaterThanOrEqualTo(1), new Sample(), ValidationErrorCodes.GreaterThanOrEqual),
            (v => v.RuleFor(x => x.Number).LessThan(0), new Sample(), ValidationErrorCodes.LessThan),
            (v => v.RuleFor(x => x.Number).LessThanOrEqualTo(-1), new Sample(), ValidationErrorCodes.LessThanOrEqual),
            (v => v.RuleFor(x => x.Number).Equal(1), new Sample(), ValidationErrorCodes.Equal),
            (v => v.RuleFor(x => x.Number).NotEqual(0), new Sample(), ValidationErrorCodes.NotEqual),
            (v => v.RuleFor(x => x.Kind).IsInEnum(), new Sample { Kind = (Kind)5 }, ValidationErrorCodes.Enum),
            (v => v.RuleFor(x => x.Number).ExclusiveBetween(0, 1), new Sample(), ValidationErrorCodes.Condition),
        };

        foreach (var (rules, sample, code) in cases)
        {
            Assert.AreEqual(code, (await One(rules, sample)).Code);
        }
    }

    [TestMethod]
    public async Task Own_Codes_Follow_The_Same_Form_As_Results()
    {
        foreach (var good in new[] { "AZ.Zaz09", "A.B.C", "M1.X9" })
        {
            Assert.AreEqual(good, (await One(v => v.RuleFor(x => x.Text).NotNull().WithErrorCode(good), new Sample())).Code);
        }

        foreach (var bad in new[] { "Pedidos", "pedidos.X", "Pedidos.x", "Pedidos..X", "Pedidos.X-Y", "Pedidos.Ñu", "1Pedidos.X", "Pedidos.", "Pe[dido.X", "Pedidos.X{", "Pedidos.X`", "Pedidos.X@", "Pedidos.X:", "Pedidos.X/" })
        {
            Assert.AreEqual(ValidationErrorCodes.Condition, (await One(v => v.RuleFor(x => x.Text).NotNull().WithErrorCode(bad), new Sample())).Code);
        }
    }

    [TestMethod]
    public async Task Odd_Property_Names_Still_Give_A_Path()
    {
        Assert.AreEqual("lista.3", (await One(v => v.RuleFor(x => x.Text).NotNull().OverridePropertyName("Lista[3"), new Sample())).Field);
        Assert.AreEqual("lista", (await One(v => v.RuleFor(x => x.Text).NotNull().OverridePropertyName("Lista[]"), new Sample())).Field);
        Assert.AreEqual("a.b", (await One(v => v.RuleFor(x => x.Text).NotNull().OverridePropertyName("A..B"), new Sample())).Field);
        Assert.AreEqual("datos.Clave X.valor", (await One(v => v.RuleFor(x => x.Text).NotNull().OverridePropertyName("Datos[Clave X].Valor"), new Sample())).Field);
    }

    [TestMethod]
    public async Task Empty_Message_Gets_The_Default()
    {
        var culture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo("es-ES");
            Assert.AreEqual("El campo 'text' no es válido.", (await One(v => v.RuleFor(x => x.Text).NotNull().WithMessage(" "), new Sample())).Message);

            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
            Assert.AreEqual("The field 'text' is not valid.", (await One(v => v.RuleFor(x => x.Text).NotNull().WithMessage(" "), new Sample())).Message);
        }
        finally
        {
            CultureInfo.CurrentUICulture = culture;
        }
    }

    [TestMethod]
    public async Task Failures_Without_Field_Are_Joined_After_The_General_Message()
    {
        var customer = Good();
        customer.Name = "raiz";
        var culture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo("es-ES");
            Assert.AreEqual("Hay errores de validación. El cliente no es válido.", (await Validate(customer)).Error!.Message);
        }
        finally
        {
            CultureInfo.CurrentUICulture = culture;
        }
    }

    [TestMethod]
    public async Task Null_Instance_Message_Follows_The_Culture()
    {
        var culture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo("es-ES");
            Assert.AreEqual("El objeto a validar (Customer) es nulo.", (await Validate(null!)).Error!.Message);

            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
            Assert.AreEqual("The object to validate (Customer) is null.", (await Validate(null!)).Error!.Message);
        }
        finally
        {
            CultureInfo.CurrentUICulture = culture;
        }
    }

    [TestMethod]
    public async Task Cancelled_Token_Throws()
    {
        using var source = new System.Threading.CancellationTokenSource();
        await source.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(async () => _ = await new FluentValidationAdapter<Customer>(new[] { new CustomerValidator() }).ValidateAsync(Good(), source.Token));
    }

    [TestMethod]
    public void Abstract_Validators_Are_Not_Registered()
    {
        var services = new ServiceCollection();
        services.LoadFluentValidationAdapters(typeof(AdapterTests).Assembly);

        Assert.AreEqual(0, services.Count(d => d.ServiceType == typeof(IValidator<Item>)));
        Assert.AreEqual(0, services.Count(d => d.ServiceType == typeof(Kitpymes.Core.Validations.Abstractions.IValidator<Item>)));
    }

    [TestMethod]
    public void Needs_Exactly_One_Message_Says_How_Many()
    {
        var exception = Assert.ThrowsExactly<InvalidOperationException>(() => new FluentValidationAdapter<Customer>(new IValidator<Customer>[] { new CustomerValidator(), new InlineValidator<Customer>() }));

        Assert.AreEqual("Customer tiene 2 validadores de FluentValidation y el adaptador necesita exactamente uno.", exception.Message);
    }
}
