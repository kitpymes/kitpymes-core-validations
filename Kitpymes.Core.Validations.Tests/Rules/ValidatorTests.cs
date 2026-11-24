using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kitpymes.Core.Results;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kitpymes.Core.Validations.Tests.Rules;

[TestClass]
public class ValidatorTests
{
    private static async Task<IReadOnlyList<FieldError>> Errors<T>(Validator<T> validator, T instance)
    {
        var result = await validator.ValidateAsync(instance);

        Assert.IsTrue(result.IsFailure, "se esperaba un fallo");
        Assert.AreEqual(ErrorType.Validation, result.Error.Type);
        Assert.AreEqual(ValidationErrorCodes.Invalid, result.Error.Code);

        return result.Error.Fields;
    }

    private static async Task Valid<T>(Validator<T> validator, T instance)
    {
        var result = await validator.ValidateAsync(instance);

        Assert.IsTrue(result.IsSuccess, result.Error?.Fields.FirstOrDefault()?.ToString() ?? string.Empty);
    }

    private static async Task Single<T>(Validator<T> validator, T instance, string field, string code)
    {
        var errors = await Errors(validator, instance);

        Assert.AreEqual(1, errors.Count, string.Join(" | ", errors));
        Assert.AreEqual(field, errors[0].Field);
        Assert.AreEqual(code, errors[0].Code);
    }

    private static TestValidator<Order> For(Action<TestValidator<Order>> rules) => new (rules);

    [TestMethod]
    public async Task Valid_Order_Returns_Success()
        => await Valid(For(v => v.For(x => x.Name).Required().MaxLength(10)), new Order { Name = "Ana" });

    [TestMethod]
    public async Task Required_Fails_With_Null_Empty_Blank_And_Empty_Collection()
    {
        var validator = For(v =>
        {
            v.For(x => x.Name).Required();
            v.For(x => x.Tags).Required();
            v.For(x => x.Address).Required();
        });

        foreach (var name in new[] { null, string.Empty, "   " })
        {
            var errors = await Errors(validator, new Order { Name = name, Tags = new List<string>(), Address = null });

            Assert.AreEqual(3, errors.Count);
            Assert.AreEqual("name", errors[0].Field);
            Assert.AreEqual("tags", errors[1].Field);
            Assert.AreEqual("address", errors[2].Field);
            Assert.IsTrue(errors.All(e => e.Code == ValidationErrorCodes.Required));
        }

        await Valid(validator, new Order { Name = "x", Tags = new List<string> { "a" }, Address = new Address() });
    }

    [TestMethod]
    public async Task Required_Passes_With_Value_Types_Default()
        => await Valid(For(v => v.For(x => x.Age).Required()), new Order { Age = 0 });

    [TestMethod]
    public async Task MaxLength_And_MinLength_Check_The_Limits()
    {
        var validator = For(v => v.For(x => x.Name).MinLength(2).MaxLength(4));

        await Valid(validator, new Order { Name = "ab" });
        await Valid(validator, new Order { Name = "abcd" });
        await Valid(validator, new Order { Name = null });
        await Single(validator, new Order { Name = "a" }, "name", ValidationErrorCodes.MinLength);
        await Single(validator, new Order { Name = "abcde" }, "name", ValidationErrorCodes.MaxLength);
    }

    [TestMethod]
    public async Task Email_Name_Subdomain_And_Matches_Check_The_Format_And_Skip_Empty()
    {
        var validator = For(v =>
        {
            v.For(x => x.Email).Email();
            v.For(x => x.Name).Name();
            v.For(x => x.Zip).Subdomain();
            v.For(x => x.Field).Matches("^[0-9]+$");
        });

        await Valid(validator, new Order { Email = "ana@kitpymes.com", Name = "José Ñúñez", Zip = "mi-sub.dominio_1", Field = "123" });
        await Valid(validator, new Order { Email = string.Empty, Name = string.Empty, Zip = string.Empty, Field = string.Empty });

        var errors = await Errors(validator, new Order { Email = "ana", Name = "Ana1", Zip = "sub dominio", Field = "12a" });

        CollectionAssert.AreEqual(
            new[] { ValidationErrorCodes.Email, ValidationErrorCodes.Name, ValidationErrorCodes.Subdomain, ValidationErrorCodes.Format },
            errors.Select(e => e.Code).ToArray());
        CollectionAssert.AreEqual(new[] { "email", "name", "codigo_postal", "field" }, errors.Select(e => e.Field).ToArray());
    }

    [TestMethod]
    public async Task Comparisons_Check_The_Limits()
    {
        await Single(For(v => v.For(x => x.Age).GreaterThan(5)), new Order { Age = 5 }, "age", ValidationErrorCodes.GreaterThan);
        await Valid(For(v => v.For(x => x.Age).GreaterThan(5)), new Order { Age = 6 });
        await Single(For(v => v.For(x => x.Age).GreaterThanOrEqual(5)), new Order { Age = 4 }, "age", ValidationErrorCodes.GreaterThanOrEqual);
        await Valid(For(v => v.For(x => x.Age).GreaterThanOrEqual(5)), new Order { Age = 5 });
        await Single(For(v => v.For(x => x.Age).LessThan(5)), new Order { Age = 5 }, "age", ValidationErrorCodes.LessThan);
        await Valid(For(v => v.For(x => x.Age).LessThan(5)), new Order { Age = 4 });
        await Single(For(v => v.For(x => x.Age).LessThanOrEqual(5)), new Order { Age = 6 }, "age", ValidationErrorCodes.LessThanOrEqual);
        await Valid(For(v => v.For(x => x.Age).LessThanOrEqual(5)), new Order { Age = 5 });

        var between = For(v => v.For(x => x.Age).Between(1, 3));
        await Valid(between, new Order { Age = 1 });
        await Valid(between, new Order { Age = 3 });
        await Single(between, new Order { Age = 0 }, "age", ValidationErrorCodes.Between);
        await Single(between, new Order { Age = 4 }, "age", ValidationErrorCodes.Between);
    }

    [TestMethod]
    public async Task Comparisons_Skip_Null_And_Reject_Null_Limits()
    {
        await Valid(For(v => v.For(x => x.Optional).GreaterThan(5).Between(1, 2)), new Order { Optional = null });
        await Single(For(v => v.For(x => x.Optional).GreaterThan(5)), new Order { Optional = 5 }, "optional", ValidationErrorCodes.GreaterThan);
        Assert.ThrowsExactly<ArgumentNullException>(() => For(v => v.For(x => x.Optional).LessThan(null)));
    }

    [TestMethod]
    public void Between_With_Min_Greater_Than_Max_Throws()
        => Assert.ThrowsExactly<ArgumentException>(() => For(v => v.For(x => x.Age).Between(3, 1)));

    [TestMethod]
    public async Task Dates_Can_Be_Compared_Between_Fields()
    {
        var validator = For(v => v.For(x => x.To).Must((order, to) => to > order.From));
        var now = new DateTime(2026, 11, 24, 0, 0, 0, DateTimeKind.Utc);

        await Valid(validator, new Order { From = now, To = now.AddDays(1) });
        await Single(validator, new Order { From = now, To = now }, "to", ValidationErrorCodes.Condition);
    }

    [TestMethod]
    public async Task Equal_And_NotEqual_Also_Check_Null()
    {
        await Single(For(v => v.For(x => x.Name).Equal("a")), new Order { Name = null }, "name", ValidationErrorCodes.Equal);
        await Valid(For(v => v.For(x => x.Name).Equal("a")), new Order { Name = "a" });
        await Single(For(v => v.For(x => x.Name).NotEqual(null)), new Order { Name = null }, "name", ValidationErrorCodes.NotEqual);
        await Valid(For(v => v.For(x => x.Name).NotEqual("a")), new Order { Name = "b" });
    }

    [TestMethod]
    public async Task IsInEnum_Checks_Defined_Values_And_Flags()
    {
        await Valid(For(v => v.For(x => x.Status).IsInEnum()), new Order { Status = Status.Sent });
        await Single(For(v => v.For(x => x.Status).IsInEnum()), new Order { Status = (Status)7 }, "status", ValidationErrorCodes.Enum);
        await Valid(For(v => v.For(x => x.MaybeStatus).IsInEnum()), new Order { MaybeStatus = null });
        await Single(For(v => v.For(x => x.MaybeStatus).IsInEnum()), new Order { MaybeStatus = (Status)7 }, "maybeStatus", ValidationErrorCodes.Enum);
        await Valid(For(v => v.For(x => x.Days).IsInEnum()), new Order { Days = Days.Monday | Days.Tuesday });
        await Single(For(v => v.For(x => x.Days).IsInEnum()), new Order { Days = (Days)4 }, "days", ValidationErrorCodes.Enum);
    }

    [TestMethod]
    public void Rules_On_The_Wrong_Type_Throw_When_Configured()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => For(v => v.For(x => x.Age).MaxLength(1)));
        Assert.ThrowsExactly<InvalidOperationException>(() => For(v => v.For(x => x.Age).Email()));
        Assert.ThrowsExactly<InvalidOperationException>(() => For(v => v.For(x => x.Age).IsInEnum()));
        Assert.ThrowsExactly<InvalidOperationException>(() => For(v => v.For(x => x.Address).GreaterThan(new Address())));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => For(v => v.For(x => x.Name).MaxLength(-1)));
        Assert.ThrowsExactly<ArgumentException>(() => For(v => v.For(x => x.Method())));
    }

    [TestMethod]
    public async Task Must_Is_Evaluated_Also_With_Null()
    {
        var calls = 0;
        await Single(For(v => v.For(x => x.Name).Must(n => { calls++; return n is not null; })), new Order(), "name", ValidationErrorCodes.Condition);
        Assert.AreEqual(1, calls);
    }

    [TestMethod]
    public async Task Custom_Adds_Several_Errors_With_Own_Codes()
    {
        var validator = For(v => v.For(x => x.Name).Custom((name, context) =>
        {
            context.AddError("Falta un número.", "Usuarios.SinNumero");
            context.AddError("Falta una mayúscula.");
        }));

        var errors = await Errors(validator, new Order { Name = "abc" });

        Assert.AreEqual(2, errors.Count);
        Assert.AreEqual("Usuarios.SinNumero", errors[0].Code);
        Assert.AreEqual("Falta un número.", errors[0].Message);
        Assert.AreEqual(ValidationErrorCodes.Condition, errors[1].Code);
        Assert.IsTrue(errors.All(e => e.Field == "name"));
    }

    [TestMethod]
    public async Task Custom_Rejects_Bad_Codes_And_Empty_Messages()
    {
        foreach (var (message, code) in new (string, string?)[] { ("m", "sinpunto"), ("m", "Validations.Propio"), ("m", "Mod.con espacio"), (" ", null) })
        {
            var validator = For(v => v.For(x => x.Name).Custom((_, context) => context.AddError(message, code)));
            await Assert.ThrowsExactlyAsync<ArgumentException>(async () => await validator.ValidateAsync(new Order()));
        }
    }

    [TestMethod]
    public async Task Stops_At_First_Error_Of_A_Field_Unless_ContinueOnError()
    {
        var stop = For(v =>
        {
            v.For(x => x.Name).MinLength(5).Email();
            v.For(x => x.Email).Required();
        });

        var errors = await Errors(stop, new Order { Name = "ab" });
        CollectionAssert.AreEqual(new[] { ValidationErrorCodes.MinLength, ValidationErrorCodes.Required }, errors.Select(e => e.Code).ToArray());

        var all = For(v => v.For(x => x.Name).MinLength(5).Email().ContinueOnError());
        errors = await Errors(all, new Order { Name = "ab" });
        CollectionAssert.AreEqual(new[] { ValidationErrorCodes.MinLength, ValidationErrorCodes.Email }, errors.Select(e => e.Code).ToArray());
    }

    [TestMethod]
    public async Task WithMessage_And_WithErrorCode_Change_The_Last_Rule()
    {
        var validator = For(v => v.For(x => x.Name).Required().WithMessage("Pon el nombre.").WithErrorCode("Pedidos.SinNombre").MaxLength(1).ContinueOnError());

        var errors = await Errors(validator, new Order { Name = " " });

        Assert.AreEqual(1, errors.Count);
        Assert.AreEqual("Pon el nombre.", errors[0].Message);
        Assert.AreEqual("Pedidos.SinNombre", errors[0].Code);

        errors = await Errors(validator, new Order { Name = "ab" });
        Assert.AreEqual(ValidationErrorCodes.MaxLength, errors[0].Code);
    }

    [TestMethod]
    public void Modifiers_Without_A_Rule_Or_With_Bad_Values_Throw()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => For(v => v.For(x => x.Name).WithMessage("m")));
        Assert.ThrowsExactly<InvalidOperationException>(() => For(v => v.For(x => x.Name).Custom((_, _) => { }).WithErrorCode("A.B")));
        Assert.ThrowsExactly<ArgumentException>(() => For(v => v.For(x => x.Name).Required().WithErrorCode("Validations.Propio")));
        Assert.ThrowsExactly<ArgumentException>(() => For(v => v.For(x => x.Name).Required().WithErrorCode("propio.codigo")));
        Assert.ThrowsExactly<ArgumentException>(() => For(v => v.For(x => x.Name).Required().WithMessage(" ")));
        Assert.ThrowsExactly<ArgumentException>(() => For(v => v.For(x => x.Name).WithName(" ")));
    }

    [TestMethod]
    public async Task When_Skips_The_Rules_If_False()
    {
        var validator = For(v => v.For(x => x.Name).Required().When(x => x.Age > 18));

        await Valid(validator, new Order { Age = 18 });
        await Single(validator, new Order { Age = 19 }, "name", ValidationErrorCodes.Required);
    }

    [TestMethod]
    public async Task Nested_Expression_Gives_Path_And_Null_Intermediate_Counts_As_Null()
    {
        var validator = For(v => v.For(x => x.Address!.Street).Required());

        await Single(validator, new Order { Address = null }, "address.street", ValidationErrorCodes.Required);
        await Single(validator, new Order { Address = new Address() }, "address.street", ValidationErrorCodes.Required);
        await Valid(validator, new Order { Address = new Address { Street = "x" } });
    }

    [TestMethod]
    public async Task SetValidator_Prefixes_The_Child_Paths_And_Skips_Null()
    {
        var validator = For(v => v.For(x => x.Address).SetValidator(new AddressValidator()));

        await Valid(validator, new Order { Address = null });
        await Single(validator, new Order { Address = new Address() }, "address.street", ValidationErrorCodes.Required);
        await Single(validator, new Order { Address = new Address { Street = "123456" } }, "address.street", ValidationErrorCodes.MaxLength);
    }

    [TestMethod]
    public async Task SetValidator_Is_Not_Run_If_A_Rule_Of_The_Field_Failed()
    {
        var validator = For(v => v.For(x => x.Address).Must(a => a!.Street != "no").SetValidator(new AddressValidator()));

        await Single(validator, new Order { Address = new Address { Street = "no" } }, "address", ValidationErrorCodes.Condition);
    }

    [TestMethod]
    public async Task RuleForEach_Gives_The_Index_And_Null_Elements_Are_Required()
    {
        var validator = For(v => v.ForEach(x => x.Lines).SetValidator(new LineValidator()));

        await Valid(validator, new Order { Lines = null });
        await Valid(validator, new Order { Lines = new List<Line> { new Line { Name = "a", Quantity = 1 } } });

        var errors = await Errors(validator, new Order { Lines = new List<Line> { new Line { Name = "a", Quantity = 1 }, null!, new Line { Quantity = 0 } } });

        CollectionAssert.AreEqual(new[] { "lines.1", "lines.2.name", "lines.2.quantity" }, errors.Select(e => e.Field).ToArray());
        CollectionAssert.AreEqual(
            new[] { ValidationErrorCodes.Required, ValidationErrorCodes.Required, ValidationErrorCodes.GreaterThan },
            errors.Select(e => e.Code).ToArray());
    }

    [TestMethod]
    public async Task RuleForEach_Applies_Rules_To_Each_Element()
    {
        var validator = For(v => v.ForEach(x => x.Tags).Required().MaxLength(2));

        var errors = await Errors(validator, new Order { Tags = new List<string> { "ok", "largo", " " } });

        CollectionAssert.AreEqual(new[] { "tags.1", "tags.2" }, errors.Select(e => e.Field).ToArray());
        CollectionAssert.AreEqual(new[] { ValidationErrorCodes.MaxLength, ValidationErrorCodes.Required }, errors.Select(e => e.Code).ToArray());
    }

    [TestMethod]
    public async Task Cycles_Are_Not_Validated_Twice()
    {
        var a = new Address { Street = "a" };
        var b = new Address { Street = "b", Next = a };
        a.Next = b;

        await Valid(new ChainValidator(), a);

        a.Street = null;
        b.Street = null;
        var errors = await Errors(new ChainValidator(), a);

        CollectionAssert.AreEqual(new[] { "street", "next.street" }, errors.Select(e => e.Field).ToArray());
    }

    [TestMethod]
    public async Task Too_Deep_Gives_A_Depth_Error_Instead_Of_Overflowing()
    {
        var root = new Address { Street = "0" };
        var current = root;

        for (var i = 1; i <= 40; i++)
        {
            current.Next = new Address { Street = i.ToString(CultureInfo.InvariantCulture) };
            current = current.Next;
        }

        var errors = await Errors(new ChainValidator(), root);

        Assert.AreEqual(1, errors.Count);
        Assert.AreEqual(ValidationErrorCodes.Depth, errors[0].Code);
        Assert.AreEqual(string.Join(".", Enumerable.Repeat("next", 33)), errors[0].Field);
    }

    [TestMethod]
    public async Task Include_Adds_The_Rules_Of_Another_Validator()
    {
        var common = For(v => v.For(x => x.Name).Required());
        var validator = For(v =>
        {
            v.Add(common);
            v.For(x => x.Email).Required();
        });

        var errors = await Errors(validator, new Order());

        CollectionAssert.AreEqual(new[] { "name", "email" }, errors.Select(e => e.Field).ToArray());
        Assert.ThrowsExactly<ArgumentException>(() => For(v => v.Add(v)));
    }

    [TestMethod]
    public async Task Root_Rule_Needs_WithName()
    {
        var bad = For(v => v.For(x => x).Must(x => x.From <= x.To));
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () => await bad.ValidateAsync(new Order()));

        var good = For(v => v.For(x => x).Must(x => x.From <= x.To).WithName("to"));
        await Single(good, new Order { From = DateTime.MaxValue }, "to", ValidationErrorCodes.Condition);
    }

    [TestMethod]
    public async Task Root_Rule_In_A_Child_Uses_The_Parent_Path()
    {
        var child = new TestValidator<Address>(v => v.For(x => x).Must(a => a.Street is not null));
        var validator = For(v => v.For(x => x.Address).SetValidator(child));

        await Single(validator, new Order { Address = new Address() }, "address", ValidationErrorCodes.Condition);
    }

    [TestMethod]
    public async Task Null_Instance_Fails_Without_Fields()
    {
        var result = await For(v => v.For(x => x.Name).Required()).ValidateAsync(null!);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(ValidationErrorCodes.Invalid, result.Error.Code);
        Assert.AreEqual(0, result.Error.Fields.Count);
    }

    [TestMethod]
    public async Task Messages_Follow_The_Culture_At_Validation_Time()
    {
        var validator = For(v => v.For(x => x.Name).Required());
        var culture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo("es-ES");
            var result = await validator.ValidateAsync(new Order());
            Assert.AreEqual("Hay errores de validación.", result.Error!.Message);
            Assert.AreEqual("El campo 'name' es obligatorio.", result.Error.Fields[0].Message);

            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
            result = await validator.ValidateAsync(new Order());
            Assert.AreEqual("There are validation errors.", result.Error!.Message);
            Assert.AreEqual("The field 'name' is required.", result.Error.Fields[0].Message);
        }
        finally
        {
            CultureInfo.CurrentUICulture = culture;
        }
    }

    [TestMethod]
    public async Task WithName_Changes_The_Field()
        => await Single(For(v => v.For(x => x.Name).Required().WithName("cliente.nombre")), new Order(), "cliente.nombre", ValidationErrorCodes.Required);

    [TestMethod]
    public async Task Cancelled_Token_Throws()
    {
        using var source = new CancellationTokenSource();
        await source.CancelAsync();

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(async () => await For(v => v.For(x => x.Name).Required()).ValidateAsync(new Order(), source.Token));
    }

    [TestMethod]
    public async Task The_Same_Validator_Can_Be_Used_From_Several_Threads()
    {
        var validator = For(v => v.For(x => x.Name).Required().MaxLength(3));

        var results = await Task.WhenAll(Enumerable.Range(0, 200).Select(i => Task.Run(async () =>
            (i, await validator.ValidateAsync(new Order { Name = i % 2 == 0 ? "ok" : "demasiado" })))));

        foreach (var (i, result) in results)
        {
            Assert.AreEqual(i % 2 == 0, result.IsSuccess);
            Assert.AreEqual(i % 2 == 0 ? 0 : 1, result.Error?.Fields.Count ?? 0);
        }
    }
}
