// -----------------------------------------------------------------------
// <copyright file="ValidationsServiceCollectionExtensions.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kitpymes.Core.Mediator.Abstractions;
using Kitpymes.Core.Validations.Abstractions;
using Kitpymes.Core.Validations.Pipeline;
using Kitpymes.Core.Validations.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/*
    Clase de extensión ValidationsServiceCollectionExtensions
    Contiene las extensiones de los servicios de las validaciones
*/

/// <summary>
/// Clase de extensión <c>ValidationsServiceCollectionExtensions</c>.
/// Contiene las extensiones de los servicios de las validaciones.
/// </summary>
/// <remarks>
/// <para>En esta clase se pueden agregar todas las extensiones de los servicios para las validaciones.</para>
/// </remarks>
public static class ValidationsServiceCollectionExtensions
{
    /// <summary>
    /// Carga el servicio de validaciones.
    /// </summary>
    /// <remarks>
    /// <para>Con <see cref="ValidationsOptions.WithAssemblies"/>, que es obligatorio, registra cada validador de los ensamblados como
    /// <see cref="IValidator{T}"/> (singleton: un validador no guarda nada entre validaciones), el paso de validación
    /// del mediador para todos los comandos y consultas y su comprobación de arranque
    /// (<see cref="IValidationStartupCheck"/>). Llamarlo dos veces no registra nada dos veces.</para>
    /// </remarks>
    /// <param name="services">Colección de servicios.</param>
    /// <param name="options">Configuración de las validaciones.</param>
    /// <returns>IServiceCollection.</returns>
    /// <exception cref="ArgumentException">Si no se indica ningún ensamblado con <see cref="ValidationsOptions.WithAssemblies"/>.</exception>
    public static IServiceCollection LoadValidations(this IServiceCollection services, Action<ValidationsOptions> options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        var configured = new ValidationsOptions();
        options(configured);

        if (configured.Assemblies.Count == 0)
        {
            throw new ArgumentException("Indique los ensamblados de los validadores con o => o.WithAssemblies(...).", nameof(options));
        }

        AddValidators(services, configured.Assemblies);

        return services;
    }

    private static void AddValidators(IServiceCollection services, IEnumerable<Assembly> assemblies)
    {
        foreach (var type in assemblies.SelectMany(LoadableTypes).Where(t => t.IsClass && !t.IsAbstract && !t.ContainsGenericParameters))
        {
            foreach (var contract in type.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>)))
            {
                services.TryAddEnumerable(ServiceDescriptor.Singleton(contract, type));
            }
        }

        services.TryAddEnumerable(ServiceDescriptor.Transient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>)));
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidationStartupCheck, ValidationStartupCheck>());
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
