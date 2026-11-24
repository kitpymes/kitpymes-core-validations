using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Kitpymes.Core.Validations.Tests.Rules;

public enum Status
{
    Draft = 0,
    Sent = 1,
}

[Flags]
public enum Days
{
    None = 0,
    Monday = 1,
    Tuesday = 2,
}

public readonly struct Money : IComparable<Money>
{
    public Money(decimal amount) => Amount = amount;

    public decimal Amount { get; }

    public int CompareTo(Money other) => Amount.CompareTo(other.Amount);

    public override string ToString() => Amount.ToString(System.Globalization.CultureInfo.InvariantCulture) + " EUR";
}

public sealed class Address
{
    public string? Street { get; set; }

    public Address? Next { get; set; }
}

public sealed class Line
{
    public string? Name { get; set; }

    public int Quantity { get; set; }
}

public sealed class Order
{
    public string? Name { get; set; }

    public string? Email { get; set; }

    public int Age { get; set; }

    public int? Optional { get; set; }

    public Status Status { get; set; }

    public Status? MaybeStatus { get; set; }

    public Days Days { get; set; }

    public Address? Address { get; set; }

    public List<Line>? Lines { get; set; }

    public List<string>? Tags { get; set; }

    [JsonPropertyName("codigo_postal")]
    public string? Zip { get; set; }

    public DateTime From { get; set; }

    public DateTime To { get; set; }

    public object? Thing { get; set; }

    public Money Price { get; set; }

    public string? Field;

    public string Method() => Name ?? string.Empty;
}

public sealed class AddressValidator : Validator<Address>
{
    public AddressValidator() => RuleFor(x => x.Street).Required().MaxLength(5);
}

public sealed class LineValidator : Validator<Line>
{
    public LineValidator()
    {
        RuleFor(x => x.Name).Required();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}

/// <summary>Validador recursivo para los ciclos y la profundidad.</summary>
public sealed class ChainValidator : Validator<Address>
{
    public ChainValidator()
    {
        RuleFor(x => x.Street).Required();
        RuleFor(x => x.Next).SetValidator(this);
    }
}

/// <summary>Validador que se arma en cada test.</summary>
public sealed class TestValidator<T> : Validator<T>
{
    public TestValidator(Action<TestValidator<T>> rules) => rules(this);

    public RuleBuilder<T, TProperty> For<TProperty>(System.Linq.Expressions.Expression<Func<T, TProperty>> expression) => RuleFor(expression);

    public RuleBuilder<T, TItem> ForEach<TItem>(System.Linq.Expressions.Expression<Func<T, IEnumerable<TItem>?>> expression) => RuleForEach(expression);

    public void Add(Validator<T> validator) => Include(validator);
}
