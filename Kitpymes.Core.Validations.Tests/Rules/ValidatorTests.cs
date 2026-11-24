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
        Assert.ThrowsExactly<InvalidOperationException>(() => For(v => v.For(x => x.Age).MinLength(1)));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => For(v => v.For(x => x.Name).MinLength(-1)));
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
            await Assert.ThrowsExactlyAsync<ArgumentException>(async () => _ = await validator.ValidateAsync(new Order()));
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
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () => _ = await bad.ValidateAsync(new Order()));

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

        var pending = For(v => v.For(x => x.Name).Required()).ValidateAsync(new Order(), source.Token);

        Assert.IsTrue(pending.IsCanceled);
        await Assert.ThrowsAsync<OperationCanceledException>(async () => _ = await pending);
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

    [TestMethod]
    public async Task Default_Messages_In_Spanish_And_English()
    {
        var cases = new (Action<TestValidator<Order>> Rules, Order Order, string Es, string En)[]
        {
            (v => v.For(x => x.Name).Required(), new Order(), "El campo 'name' es obligatorio.", "The field 'name' is required."),
            (v => v.For(x => x.Name).MaxLength(1), new Order { Name = "ab" }, "El campo 'name' debe tener como máximo 1 caracteres.", "The field 'name' must have at most 1 characters."),
            (v => v.For(x => x.Name).MinLength(3), new Order { Name = "ab" }, "El campo 'name' debe tener como mínimo 3 caracteres.", "The field 'name' must have at least 3 characters."),
            (v => v.For(x => x.Name).Email(), new Order { Name = "ab" }, "El campo 'name' no es un email válido.", "The field 'name' is not a valid email."),
            (v => v.For(x => x.Name).Name(), new Order { Name = "a1" }, "El campo 'name' no es un nombre válido.", "The field 'name' is not a valid name."),
            (v => v.For(x => x.Name).Subdomain(), new Order { Name = "a b" }, "El campo 'name' no es un subdominio válido.", "The field 'name' is not a valid subdomain."),
            (v => v.For(x => x.Name).Matches("^x$"), new Order { Name = "a" }, "El campo 'name' no tiene un formato válido.", "The field 'name' does not have a valid format."),
            (v => v.For(x => x.Age).GreaterThan(1), new Order(), "El campo 'age' debe ser mayor que 1.", "The field 'age' must be greater than 1."),
            (v => v.For(x => x.Age).GreaterThanOrEqual(1), new Order(), "El campo 'age' debe ser mayor o igual que 1.", "The field 'age' must be greater than or equal to 1."),
            (v => v.For(x => x.Age).LessThan(0), new Order(), "El campo 'age' debe ser menor que 0.", "The field 'age' must be less than 0."),
            (v => v.For(x => x.Age).LessThanOrEqual(-1), new Order(), "El campo 'age' debe ser menor o igual que -1.", "The field 'age' must be less than or equal to -1."),
            (v => v.For(x => x.Age).Between(1, 2), new Order(), "El campo 'age' debe estar entre 1 y 2.", "The field 'age' must be between 1 and 2."),
            (v => v.For(x => x.Age).Equal(1), new Order(), "El campo 'age' debe ser 1.", "The field 'age' must be 1."),
            (v => v.For(x => x.Age).NotEqual(0), new Order(), "El campo 'age' no puede ser 0.", "The field 'age' cannot be 0."),
            (v => v.For(x => x.Status).IsInEnum(), new Order { Status = (Status)9 }, "El campo 'status' tiene un valor que no es válido.", "The field 'status' has a value that is not valid."),
            (v => v.For(x => x.Age).Must(a => a > 0), new Order(), "El campo 'age' no es válido.", "The field 'age' is not valid."),
        };

        var culture = CultureInfo.CurrentUICulture;

        try
        {
            foreach (var (rules, order, es, en) in cases)
            {
                var validator = For(rules);

                CultureInfo.CurrentUICulture = new CultureInfo("es-ES");
                Assert.AreEqual(es, (await Errors(validator, order))[0].Message);

                CultureInfo.CurrentUICulture = new CultureInfo("en-GB");
                Assert.AreEqual(en, (await Errors(validator, order))[0].Message);
            }

            var root = new Address { Street = "0" };
            var current = root;

            for (var i = 0; i < 40; i++)
            {
                current.Next = new Address { Street = "x" };
                current = current.Next;
            }

            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
            StringAssert.Contains((await Errors(new ChainValidator(), root))[0].Message, "is nested more than 32 levels deep.");
            Assert.AreEqual("The object to validate (Order) is null.", (await For(v => v.For(x => x.Name).Required()).ValidateAsync(null!)).Error!.Message);

            CultureInfo.CurrentUICulture = new CultureInfo("es-ES");
            StringAssert.Contains((await Errors(new ChainValidator(), root))[0].Message, "está anidado a más de 32 niveles.");
            Assert.AreEqual("El objeto a validar (Order) es nulo.", (await For(v => v.For(x => x.Name).Required()).ValidateAsync(null!)).Error!.Message);
        }
        finally
        {
            CultureInfo.CurrentUICulture = culture;
        }
    }

    [TestMethod]
    public void Own_Codes_Must_Be_Module_Dot_Reason_PascalCase_Ascii()
    {
        foreach (var good in new[] { "Pedidos.SinNombre", "AZ.Zaz09", "A.B.C", "M1.X9" })
        {
            For(v => v.For(x => x.Name).Required().WithErrorCode(good));
        }

        foreach (var bad in new[] { "Pedidos", "pedidos.SinNombre", "Pedidos.sinNombre", "Pedidos..X", "Pedidos.X-Y", "Pedidos.Ñu", "1Pedidos.X", "Pedidos.X ", ".Pedidos", "Pedidos.", "Pe[dido.X", "Pedidos.X{", "Pedidos.X`", "Pedidos.X@", "Pedidos.X:", "Pedidos.X/" })
        {
            Assert.ThrowsExactly<ArgumentException>(() => For(v => v.For(x => x.Name).Required().WithErrorCode(bad)));
        }

        Assert.ThrowsExactly<ArgumentException>(() => For(v => v.For(x => x.Name).Required().WithErrorCode(null!)));
    }

    [TestMethod]
    public void Null_Arguments_Throw_When_Configured()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => For(v => v.For(x => x.Name).Must((Func<string?, bool>)null!)));
        Assert.ThrowsExactly<ArgumentNullException>(() => For(v => v.For(x => x.Name).Must((Func<Order, string?, bool>)null!)));
        Assert.ThrowsExactly<ArgumentNullException>(() => For(v => v.For(x => x.Name).Custom(null!)));
        Assert.ThrowsExactly<ArgumentNullException>(() => For(v => v.For(x => x.Address).SetValidator((Validator<Address>)null!)));
        Assert.ThrowsExactly<ArgumentNullException>(() => For(v => v.For(x => x.Name).When(null!)));
        Assert.ThrowsExactly<ArgumentNullException>(() => For(v => v.Add(null!)));
        Assert.ThrowsExactly<ArgumentNullException>(() => For(v => v.For<string?>(null!)));
        Assert.ThrowsExactly<ArgumentNullException>(() => For(v => v.For(x => x.Name).Matches(null!)));
        Assert.ThrowsExactly<ArgumentException>(() => For(v => v.For(x => x.Name).Matches(string.Empty)));
    }

    [TestMethod]
    public void SetValidator_Needs_A_Validator_Of_The_Same_Type()
    {
        var exception = Assert.ThrowsExactly<InvalidOperationException>(() => For(v => v.For(x => x.Thing).SetValidator(new AddressValidator())));

        StringAssert.Contains(exception.Message, "'thing'");
    }

    [TestMethod]
    public async Task Between_Accepts_Equal_Limits_And_Needs_Comparable_Values()
    {
        await Valid(For(v => v.For(x => x.Age).Between(1, 1)), new Order { Age = 1 });
        Assert.ThrowsExactly<InvalidOperationException>(() => For(v => v.For(x => x.Address).Between(new Address(), new Address())));
    }

    [TestMethod]
    public async Task Values_With_Only_Generic_IComparable_Can_Be_Compared()
    {
        var validator = For(v => v.For(x => x.Price).GreaterThan(new Money(10)));

        await Valid(validator, new Order { Price = new Money(11) });
        var errors = await Errors(validator, new Order { Price = new Money(10) });
        StringAssert.Contains(errors[0].Message, "10 EUR");
    }

    [TestMethod]
    public async Task Boxed_Values_Keep_The_Path()
        => await Single(For(v => v.For<object?>(x => x.Age).Must(a => (int)a! > 0)), new Order(), "age", ValidationErrorCodes.Condition);

    [TestMethod]
    public async Task Patterns_Skip_Null()
        => await Valid(For(v => v.For(x => x.Email).Email().Name().Subdomain().Matches("^x$")), new Order { Email = null });

    [TestMethod]
    public async Task ContinueOnError_Also_Runs_The_Child_Validator()
    {
        var validator = For(v => v.For(x => x.Address).Must(a => a!.Street is not null).SetValidator(new AddressValidator()).ContinueOnError());

        var errors = await Errors(validator, new Order { Address = new Address() });

        CollectionAssert.AreEqual(new[] { "address", "address.street" }, errors.Select(e => e.Field).ToArray());
    }

    [TestMethod]
    public async Task The_Same_Object_In_Two_Places_Is_Validated_In_Both()
    {
        var line = new Line();
        var validator = For(v => v.ForEach(x => x.Lines).SetValidator(new LineValidator()));

        var errors = await Errors(validator, new Order { Lines = new List<Line> { line, line } });

        CollectionAssert.AreEqual(
            new[] { "lines.0.name", "lines.0.quantity", "lines.1.name", "lines.1.quantity" },
            errors.Select(e => e.Field).ToArray());
    }

    [TestMethod]
    public async Task Many_Siblings_Do_Not_Count_As_Depth()
    {
        var validator = For(v => v.ForEach(x => x.Lines).SetValidator(new LineValidator()));
        var lines = Enumerable.Range(0, 50).Select(_ => new Line { Name = "a" }).ToList();

        var errors = await Errors(validator, new Order { Lines = lines });

        Assert.AreEqual(50, errors.Count);
        Assert.IsTrue(errors.All(e => e.Code == ValidationErrorCodes.GreaterThan));
    }

    [TestMethod]
    public async Task Rules_That_Throw_Do_So_When_Awaited()
    {
        var validator = For(v => v.For(x => x.Name).Must(_ => throw new FormatException("propia")));

        var pending = validator.ValidateAsync(new Order());

        Assert.IsTrue(pending.IsFaulted);
        await Assert.ThrowsExactlyAsync<FormatException>(async () => _ = await pending);
    }

    [TestMethod]
    public async Task IsInEnum_Works_With_Negative_Members()
    {
        await Valid(For(v => v.For(x => x.Sign).IsInEnum()), new Order { Sign = Sign.Negative });
        await Single(For(v => v.For(x => x.Sign).IsInEnum()), new Order { Sign = (Sign)7 }, "sign", ValidationErrorCodes.Enum);
        await Valid(For(v => v.For(x => x.SignFlags).IsInEnum()), new Order { SignFlags = SignFlags.Low | SignFlags.High });
        await Valid(For(v => v.For(x => x.SignFlags).IsInEnum()), new Order { SignFlags = SignFlags.Low | SignFlags.Top });
        await Single(For(v => v.For(x => x.SignFlags).IsInEnum()), new Order { SignFlags = (SignFlags)4 }, "signFlags", ValidationErrorCodes.Enum);
    }

    [TestMethod]
    public async Task Value_Field_Behind_A_Null_Intermediate_Counts_As_Null()
    {
        var calls = 0;
        var validator = For(v =>
        {
            v.For(x => x.Address!.Number).Required().GreaterThan(0).ContinueOnError();
            v.For(x => x.Address!.Id).Must(_ => { calls++; return true; });
        });

        var errors = await Errors(validator, new Order { Address = null });

        Assert.AreEqual(1, errors.Count);
        Assert.AreEqual("address.number", errors[0].Field);
        Assert.AreEqual(ValidationErrorCodes.Required, errors[0].Code);
        Assert.AreEqual(0, calls);

        var custom = For(v => v.For(x => x.Address!.Number).Required().WithMessage("Falta el número.").GreaterThan(0).ContinueOnError());
        var only = await Errors(custom, new Order());
        Assert.AreEqual(1, only.Count);
        Assert.AreEqual("Falta el número.", only[0].Message);

        await Single(validator, new Order { Address = new Address { Number = 0 } }, "address.number", ValidationErrorCodes.GreaterThan);
        Assert.AreEqual(1, calls);
    }

    [TestMethod]
    public void Invalid_Pattern_Throws_When_Configured()
        => Assert.ThrowsExactly<System.Text.RegularExpressions.RegexParseException>(() => For(v => v.For(x => x.Name).Matches("(")));

    [TestMethod]
    public async Task Nullable_Value_Is_Not_Part_Of_The_Path()
        => await Single(For(v => v.For(x => x.Point!.Value.X).GreaterThan(0)), new Order { Point = new Point() }, "point.x", ValidationErrorCodes.GreaterThan);

    [TestMethod]
    public async Task Null_Element_Gives_Required_Once()
    {
        var validator = For(v => v.ForEach(x => x.Lines).Required().ContinueOnError().SetValidator(new LineValidator()));

        await Single(validator, new Order { Lines = new List<Line> { null! } }, "lines.0", ValidationErrorCodes.Required);
    }

    [TestMethod]
    public async Task Values_In_Messages_Use_The_Ui_Culture_And_Null_Is_Named()
    {
        var culture = CultureInfo.CurrentCulture;
        var ui = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("es-ES");
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
            Assert.AreEqual("The field 'amount' must be greater than 1.5.", (await Errors(For(v => v.For(x => x.Amount).GreaterThan(1.5m)), new Order()))[0].Message);
            Assert.AreEqual("The field 'name' must be null.", (await Errors(For(v => v.For(x => x.Name).Equal(null)), new Order { Name = "a" }))[0].Message);

            CultureInfo.CurrentUICulture = new CultureInfo("es-ES");
            Assert.AreEqual("El campo 'amount' debe ser mayor que 1,5.", (await Errors(For(v => v.For(x => x.Amount).GreaterThan(1.5m)), new Order()))[0].Message);
            Assert.AreEqual("El campo 'name' no puede ser nulo.", (await Errors(For(v => v.For(x => x.Name).NotEqual(null)), new Order()))[0].Message);
        }
        finally
        {
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = ui;
        }
    }

    [TestMethod]
    public async Task Texts_Are_Compared_By_Ordinal()
    {
        // Por ordinal, "a" (97) va detrás de "B" (66); con la cultura sería al revés.
        await Valid(For(v => v.For(x => x.Name).GreaterThan("B")), new Order { Name = "a" });
        await Single(For(v => v.For(x => x.Name).LessThan("B")), new Order { Name = "a" }, "name", ValidationErrorCodes.LessThan);
    }

    [TestMethod]
    public void Between_Rejects_Null_Limits()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => For(v => v.For(x => x.Optional).Between(null, 1)));
        Assert.ThrowsExactly<ArgumentNullException>(() => For(v => v.For(x => x.Optional).Between(1, null)));
    }
}
