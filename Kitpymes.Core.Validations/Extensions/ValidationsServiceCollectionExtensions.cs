// -----------------------------------------------------------------------
// <copyright file="ValidationsServiceCollectionExtensions.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations.Extensions;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Kitpymes.Core.Mediator.Abstractions;
using Kitpymes.Core.Shared.Extensions;
using Kitpymes.Core.Validations.Abstractions;
using Kitpymes.Core.Validations.FluentValidation.Extensions;
using Kitpymes.Core.Validations.Pipeline;
using Kitpymes.Core.Validations.Settings;
using Microsoft.Extensions.Configuration;
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
    /// <param name="services">Colección de servicios.</param>
    /// <returns>IServiceCollection.</returns>
    [Obsolete("Lee la configuración de la API antigua. Use LoadValidations(o => o.WithAssemblies(...)) con validadores Validator<T>. Se elimina en 3.0.0.", DiagnosticId = "KPY0007")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static IServiceCollection LoadValidations(this IServiceCollection services)
    {
        var configuration = services.ToService<IConfiguration>();

        var settings = configuration?.GetSection(nameof(ValidationsSettings))?.Get<ValidationsSettings>();

        return services.LoadValidations(settings);
    }

    /// <summary>
    /// Carga el servicio de validaciones.
    /// </summary>
    /// <remarks>
    /// <para>Con <see cref="ValidationsOptions.WithAssemblies"/> registra cada validador de los ensamblados como
    /// <see cref="IValidator{T}"/> (singleton: un validador no guarda nada entre validaciones), el paso de validación
    /// del mediador para todos los comandos y consultas y su comprobación de arranque
    /// (<see cref="IValidationStartupCheck"/>). Llamarlo dos veces no registra nada dos veces.</para>
    /// </remarks>
    /// <param name="services">Colección de servicios.</param>
    /// <param name="options">Configuración de las validaciones.</param>
    /// <returns>IServiceCollection.</returns>
    public static IServiceCollection LoadValidations(this IServiceCollection services, Action<ValidationsOptions> options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        var configured = new ValidationsOptions();
        options(configured);

        if (configured.Assemblies.Count > 0)
        {
            AddValidators(services, configured.Assemblies);
        }

#pragma warning disable KPY0007 // La API antigua sigue funcionando hasta la 3.0.0.
        return services.LoadValidations(configured.ValidationsSettings);
#pragma warning restore KPY0007
    }

    /// <summary>
    /// Carga el servicio de validaciones.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <param name="settings">Configuración de las validaciones.</param>
    /// <returns>IServiceCollection.</returns>
    [Obsolete("Configura la API antigua. Use LoadValidations(o => o.WithAssemblies(...)) con validadores Validator<T>. Se elimina en 3.0.0.", DiagnosticId = "KPY0007")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static IServiceCollection LoadValidations(this IServiceCollection services, ValidationsSettings? settings)
    {
        if (settings?.Enabled == true)
        {
            if (settings.FluentValidationSettings?.Enabled == true)
            {
                services.LoadFluentValidation(settings.FluentValidationSettings);
            }
        }

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
