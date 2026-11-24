// -----------------------------------------------------------------------
// <copyright file="ValidationStartupCheck.cs" company="Kitpymes">
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
using Kitpymes.Core.Validations.Internal;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Comprobación de arranque de la validación para Kitpymes.Core.Mediator: todo comando y consulta con algún dato
/// de entrada tiene exactamente un validador, el comportamiento de validación está registrado y ningún validador
/// está mal configurado. Si falla, la aplicación no arranca; el test de arquitectura ejecuta lo mismo.
/// </summary>
internal sealed class ValidationStartupCheck : IValidationStartupCheck
{
    /// <inheritdoc/>
    public Task<Result> CheckAsync(IMediatorCatalog catalog, IServiceProvider services, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(services);

        return cancellationToken.IsCancellationRequested
            ? Task.FromCanceled<Result>(cancellationToken)
            : Task.FromResult(Check(catalog, services));
    }

    private static Result Check(IMediatorCatalog catalog, IServiceProvider services)
    {
        var problems = new List<string>();

        foreach (var type in catalog.Commands.Concat(catalog.Queries).Distinct().OrderBy(t => t.FullName, StringComparer.Ordinal))
        {
            List<object?> validators;

            try
            {
                validators = services.GetServices(typeof(IValidator<>).MakeGenericType(type)).ToList();
            }
            catch (Exception e)
            {
                // Un validador cuyo constructor lanza (una regla mal configurada) no corta la comprobación.
                problems.Add($"{type.FullName}: no se pudo crear su validador: {e.Message}");
                continue;
            }

            if (validators.Count == 0)
            {
                if (ValidationExemption.NeedsValidator(type))
                {
                    problems.Add($"{type.FullName} no tiene validador (IValidator<{type.Name}>).");
                }

                continue;
            }

            if (validators.Count > 1)
            {
                problems.Add($"{type.FullName} tiene {validators.Count} validadores y sólo puede tener uno.");
            }

            foreach (var configuration in validators.OfType<IValidatorConfiguration>())
            {
                problems.AddRange(configuration.Problems().Select(p => $"{configuration.GetType().Name}: {p}"));
            }

            var resultType = ResultType(type);

            bool registered;

            try
            {
                registered = resultType is null || HasBehavior(services, type, resultType);
            }
            catch (Exception e)
            {
                problems.Add($"{type.FullName}: no se pudieron crear sus comportamientos: {e.Message}");
                continue;
            }

            if (!registered)
            {
                problems.Add($"El comportamiento de validación no está registrado para {type.FullName}: use services.LoadValidations(o => o.WithAssemblies(...)).");
            }
        }

        return problems.Count == 0
            ? Result.Success()
            : Result.Failure(Error.Unexpected(
                ValidationErrorCodes.Configuration,
                "La validación no está bien configurada:" + string.Concat(problems.Select(p => Environment.NewLine + "- " + p))));
    }

    private static Type? ResultType(Type request)
        => request.GetInterfaces()
            .Where(i => i.IsGenericType && (i.GetGenericTypeDefinition() == typeof(ICommand<>) || i.GetGenericTypeDefinition() == typeof(IQuery<>)))
            .Select(i => i.GetGenericArguments()[0])
            .FirstOrDefault();

    private static bool HasBehavior(IServiceProvider services, Type request, Type result)
        => services.GetServices(typeof(IPipelineBehavior<,>).MakeGenericType(request, result))
            .Any(b => b is not null && b.GetType().IsGenericType && b.GetType().GetGenericTypeDefinition() == typeof(ValidationBehavior<,>));
}
