// -----------------------------------------------------------------------
// <copyright file="Validator{T}.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Kitpymes.Core.Results;
using Kitpymes.Core.Validations.Abstractions;
using Kitpymes.Core.Validations.Internal;

/// <summary>
/// Base para escribir el validador de forma de un comando, una consulta o cualquier objeto.
/// Las reglas se declaran en el constructor con <see cref="RuleFor{TProperty}"/> y <see cref="RuleForEach{TItem}"/>.
/// </summary>
/// <remarks>
/// <para>El resultado es un <see cref="Result"/>: correcto, o un <see cref="Error"/> de validación
/// (<see cref="ValidationErrorCodes.Invalid"/>) con un <see cref="FieldError"/> por cada error. El campo va con la ruta
/// que espera un formulario de Angular: en camelCase como System.Text.Json (respeta <c>[JsonPropertyName]</c>), con
/// puntos para los objetos anidados y el índice para las colecciones (<c>address.street</c>, <c>items.0.name</c>).</para>
/// <para>Un validador no guarda nada entre validaciones: se registra como singleton y puede usarse a la vez desde
/// varios hilos. No consulta la base de datos: las reglas de negocio van en el dominio o en el manejador.</para>
/// </remarks>
/// <typeparam name="T">Tipo que se valida.</typeparam>
public abstract class Validator<T> : IValidator<T>, IValidatorConfiguration
{
    private readonly List<IRule<T>> _rules = new ();

    /// <inheritdoc/>
    public ValueTask<Result> ValidateAsync(T instance, CancellationToken cancellationToken = default)
    {
        if (instance is null)
        {
            return new ValueTask<Result>(Result.Failure(Error.Validation(ValidationErrorCodes.Invalid, Messages.NullInstance(typeof(T).Name))));
        }

        var context = new ValidationContext(cancellationToken);
        context.Root(instance);

        Collect(instance, context, string.Empty);

        return new ValueTask<Result>(context.Errors.Count == 0
            ? Result.Success()
            : Result.Failure(Error.Validation(ValidationErrorCodes.Invalid, Messages.Invalid(), context.Errors)));
    }

    /// <inheritdoc/>
    IReadOnlyList<string> IValidatorConfiguration.Problems()
        => _rules.Select(rule => rule.Problem(string.Empty)).OfType<string>().Distinct().ToList();

    internal void Collect(T instance, ValidationContext context, string prefix)
    {
        foreach (var rule in _rules)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            rule.Validate(instance, context, prefix);
        }
    }

    /// <summary>
    /// Declara las reglas de un campo (o del objeto entero con <c>x => x</c> y <c>WithName</c>).
    /// </summary>
    /// <typeparam name="TProperty">Tipo del campo.</typeparam>
    /// <param name="expression">Propiedad o campo, también anidado (<c>x => x.Address.Street</c>). Si un intermedio es nulo, el valor cuenta como nulo.</param>
    /// <returns>El constructor de reglas del campo.</returns>
    protected RuleBuilder<T, TProperty> RuleFor<TProperty>(Expression<Func<T, TProperty>> expression)
    {
        var rule = new RuleBuilder<T, TProperty>(MemberPath.From(expression), each: false);
        _rules.Add(rule);
        return rule;
    }

    /// <summary>
    /// Declara las reglas de cada elemento de una colección. Una colección nula no da error (para eso está
    /// <c>RuleFor(x => x.Items).Required()</c>).
    /// </summary>
    /// <typeparam name="TItem">Tipo de los elementos.</typeparam>
    /// <param name="expression">Propiedad o campo con la colección.</param>
    /// <returns>El constructor de reglas de los elementos; cada error lleva el índice en la ruta (<c>items.0</c>).</returns>
    protected RuleBuilder<T, TItem> RuleForEach<TItem>(Expression<Func<T, IEnumerable<TItem>?>> expression)
    {
        var rule = new RuleBuilder<T, TItem>(MemberPath.From(expression), each: true);
        _rules.Add(rule);
        return rule;
    }

    /// <summary>
    /// Añade las reglas de otro validador del mismo tipo (reglas comunes a varios comandos).
    /// </summary>
    /// <param name="validator">Validador cuyas reglas se añaden.</param>
    protected void Include(Validator<T> validator)
    {
        ArgumentNullException.ThrowIfNull(validator);

        if (ReferenceEquals(validator, this))
        {
            throw new ArgumentException("Un validador no se puede incluir a sí mismo.", nameof(validator));
        }

        _rules.Add(new IncludedRule<T>(validator));
    }
}
