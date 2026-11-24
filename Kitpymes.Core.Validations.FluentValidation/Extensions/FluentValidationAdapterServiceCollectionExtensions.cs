// -----------------------------------------------------------------------
// <copyright file="FluentValidationAdapterServiceCollectionExtensions.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations.FluentValidation.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kitpymes.Core.Validations.Abstractions;
using Kitpymes.Core.Validations.FluentValidation.Adapter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using FV = global::FluentValidation;

/// <summary>
/// Registro de los validadores de FluentValidation como <see cref="IValidator{T}"/>.
/// </summary>
public static class FluentValidationAdapterServiceCollectionExtensions
{
    /// <summary>
    /// Registra cada validador de FluentValidation de los ensamblados (scoped, como hace FluentValidation) y, para su
    /// tipo, el adaptador <see cref="FluentValidationAdapter{T}"/> como <see cref="IValidator{T}"/>.
    /// </summary>
    /// <remarks>
    /// <para>No registra el paso de validación del mediador ni su comprobación de arranque: eso lo hace
    /// <c>services.LoadValidations(o => o.WithAssemblies(...))</c>, que hay que llamar también. Un tipo sólo puede tener
    /// un validador: si tiene uno de FluentValidation y un <c>Validator&lt;T&gt;</c>, la comprobación de arranque
    /// falla. Llamarlo dos veces no registra nada dos veces.</para>
    /// </remarks>
    /// <param name="services">Colección de servicios.</param>
    /// <param name="assemblies">Ensamblados donde están los validadores de FluentValidation.</param>
    /// <returns>La colección de servicios.</returns>
    /// <exception cref="ArgumentException">Si no se indica ningún ensamblado o alguno es nulo.</exception>
    public static IServiceCollection LoadFluentValidationAdapters(this IServiceCollection services, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0)
        {
            throw new ArgumentException("Indique al menos un ensamblado con validadores.", nameof(assemblies));
        }

        if (assemblies.Any(a => a is null))
        {
            throw new ArgumentException("La lista de ensamblados no puede contener nulos.", nameof(assemblies));
        }

        foreach (var type in assemblies.Distinct().SelectMany(LoadableTypes).Where(t => t.IsClass && !t.IsAbstract && !t.ContainsGenericParameters))
        {
            foreach (var contract in type.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(FV.IValidator<>)))
            {
                var validated = contract.GetGenericArguments()[0];

                services.TryAddEnumerable(ServiceDescriptor.Scoped(contract, type));
                services.TryAddEnumerable(ServiceDescriptor.Scoped(
                    typeof(IValidator<>).MakeGenericType(validated),
                    typeof(FluentValidationAdapter<>).MakeGenericType(validated)));
            }
        }

        return services;
    }

    private static IEnumerable<Type> LoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            return e.Types.OfType<Type>();
        }
    }
}
