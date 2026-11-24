using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kitpymes.Core.Mediator.Abstractions;
using Kitpymes.Core.Results;
using Kitpymes.Core.Validations.Abstractions;

namespace Kitpymes.Core.Validations.Tests.Pipeline;

public sealed class CreateOrder : ICommand<Result<int>>
{
    public string? Name { get; set; }
}

public sealed class CreateOrderValidator : Validator<CreateOrder>
{
    public CreateOrderValidator() => RuleFor(x => x.Name).Required();
}

public sealed class Ping : IQuery<Result>
{
    private string? Hidden { get; set; }

    public static string? Shared { get; set; }

    public string this[int index] => Hidden ?? index.ToString(System.Globalization.CultureInfo.InvariantCulture);

    public string? WriteOnly { set => Hidden = value; }
}

public sealed class WithoutValidator : ICommand<Result>
{
    public string? Ignored { set => _ = value; }

    public string? Value { get; set; }
}

public sealed class WithField : ICommand<Result>
{
    public string? Value;
}

public sealed class Twice : ICommand<Result>
{
    public string? Value { get; set; }
}

public sealed class TwiceA : Validator<Twice>
{
    public TwiceA() => RuleFor(x => x.Value).Required();
}

public sealed class TwiceB : Validator<Twice>
{
    public TwiceB() => RuleFor(x => x.Value).Required();
}

public sealed class BadRoot : IQuery<Result<string>>
{
    public string? Value { get; set; }
}

public sealed class BadRootValidator : Validator<BadRoot>
{
    public BadRootValidator() => RuleFor(x => x).Must(x => x.Value is not null);
}

/// <summary>Validador escrito a mano sobre la interfaz, sin la clase base.</summary>
public sealed class ManualRequest : ICommand<Result>
{
    public string? Value { get; set; }
}

public sealed class ManualValidator : IValidator<ManualRequest>
{
    public ValueTask<Result> ValidateAsync(ManualRequest instance, CancellationToken cancellationToken = default)
        => new (instance.Value == "null" ? null! : Result.Success());
}

/// <summary>No se registra: es abstracta.</summary>
public abstract class AbstractValidatorOfCreateOrder : Validator<CreateOrder>
{
}

public sealed class Included : ICommand<Result>
{
    public string? Value { get; set; }
}

public sealed class IncludedCommon : Validator<Included>
{
    public IncludedCommon() => RuleFor(x => x.Value).Required();
}

public sealed class IncludedValidator : Validator<Included>
{
    public IncludedValidator() => Include(new IncludedCommon());
}

public sealed class OtherBehavior<TRequest, TResult> : IPipelineBehavior<TRequest, TResult>
    where TRequest : notnull
    where TResult : Result
{
    public Task<TResult> HandleAsync(TRequest request, NextHandler<TResult> next, CancellationToken cancellationToken) => next();
}

public sealed class Broken : ICommand<Result>
{
    public int Value { get; set; }
}

/// <summary>Mal configurado: MaxLength sobre un número lanza en el constructor.</summary>
public sealed class BrokenValidator : Validator<Broken>
{
    public BrokenValidator() => RuleFor(x => x.Value).MaxLength(1);
}

public sealed class BrokenBehavior<TRequest, TResult> : IPipelineBehavior<TRequest, TResult>
    where TRequest : notnull
    where TResult : Result
{
    public BrokenBehavior() => throw new InvalidOperationException("roto");

    public Task<TResult> HandleAsync(TRequest request, NextHandler<TResult> next, CancellationToken cancellationToken) => next();
}

public sealed class Catalog : IMediatorCatalog
{
    public Catalog(params Type[] commands)
        : this(commands, Array.Empty<Type>())
    {
    }

    public Catalog(Type[] commands, Type[] queries)
    {
        Commands = commands;
        Queries = queries;
    }

    public IReadOnlyCollection<Type> Commands { get; }

    public IReadOnlyCollection<Type> Queries { get; }

    public IReadOnlyCollection<Type> Notifications { get; } = Array.Empty<Type>();
}
