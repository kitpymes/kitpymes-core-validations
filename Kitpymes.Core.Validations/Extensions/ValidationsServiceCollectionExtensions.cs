// -----------------------------------------------------------------------
// <copyright file="ValidationsServiceCollectionExtensions.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations.Extensions;

using System;
using Kitpymes.Core.Shared.Extensions;
using Kitpymes.Core.Validations.FluentValidation.Extensions;
using Kitpymes.Core.Validations.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
    public static IServiceCollection LoadValidations(this IServiceCollection services)
    {
        var configuration = services.ToService<IConfiguration>();

        var settings = configuration?.GetSection(nameof(ValidationsSettings))?.Get<ValidationsSettings>();

        return services.LoadValidations(settings);
    }

    /// <summary>
    /// Carga el servicio de validaciones.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <param name="options">Configuración de las validaciones.</param>
    /// <returns>IServiceCollection.</returns>
    public static IServiceCollection LoadValidations(this IServiceCollection services, Action<ValidationsOptions> options)
    => services.LoadValidations(options.ToConfigureOrDefault().ValidationsSettings);

    /// <summary>
    /// Carga el servicio de validaciones.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <param name="settings">Configuración de las validaciones.</param>
    /// <returns>IServiceCollection.</returns>
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
}
