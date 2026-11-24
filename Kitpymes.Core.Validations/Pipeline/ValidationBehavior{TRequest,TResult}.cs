// -----------------------------------------------------------------------
// <copyright file="ValidationBehavior{TRequest,TResult}.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations.Pipeline;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kitpymes.Core.Mediator.Abstractions;
using Kitpymes.Core.Results;
using Kitpymes.Core.Validations.Abstractions;

/// <summary>
/// Paso común del mediador que valida cada comando y consulta con su <see cref="IValidator{T}"/> antes de llegar
/// al manejador. Si no es válido, devuelve el fallo de validación sin llamar al manejador.
/// </summary>
/// <remarks>
/// <para>Es un comportamiento general (<see cref="IPipelineBehavior{TRequest, TResult}"/>), así que va por fuera de
/// los comportamientos sólo de comandos, como la transacción.</para>
/// </remarks>
/// <typeparam name="TRequest">Tipo del comando o la consulta.</typeparam>
/// <typeparam name="TResult">Tipo del resultado.</typeparam>
internal sealed class ValidationBehavior<TRequest, TResult> : IPipelineBehavior<TRequest, TResult>
    where TRequest : notnull
    where TResult : Result
{
    private readonly IValidator<TRequest>[] _validators;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ValidationBehavior{TRequest, TResult}"/>.
    /// </summary>
    /// <param name="validators">Validadores registrados del comando o la consulta.</param>
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        => _validators = (validators ?? throw new ArgumentNullException(nameof(validators))).ToArray();

    /// <inheritdoc/>
    public async Task<TResult> HandleAsync(TRequest request, NextHandler<TResult> next, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        if (_validators.Length == 0)
        {
            return await next().ConfigureAwait(false);
        }

        if (_validators.Length > 1)
        {
            throw new InvalidOperationException($"{typeof(TRequest).Name} tiene {_validators.Length} validadores registrados y sólo puede tener uno.");
        }

        var result = await _validators[0].ValidateAsync(request, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"El validador {_validators[0].GetType().Name} de {typeof(TRequest).Name} devolvió un resultado nulo.");

        return result.IsFailure
            ? Result.FailureFor<TResult>(result.Error)
            : await next().ConfigureAwait(false);
    }
}
