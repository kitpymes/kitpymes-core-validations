// -----------------------------------------------------------------------
// <copyright file="ValidationsApplicationBuilderExtensions.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations.Extensions;

using Kitpymes.Core.Shared.Extensions;
using Kitpymes.Core.Validations.Middleware;
using Kitpymes.Core.Validations.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

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
public static class ValidationsApplicationBuilderExtensions
{
    /// <summary>
    /// Carga el middlware para captar los errores.
    /// </summary>
    /// <param name="application">Define una clase que proporciona los mecanismos para configurar la solicitud de una aplicación.</param>
    /// <returns>IApplicationBuilder.</returns>
    public static IApplicationBuilder LoadValidations(this IApplicationBuilder application)
    {
        var configuration = application.ToService<IConfiguration>();

        var settings = configuration?.GetSection(nameof(ValidationsSettings))?.Get<ValidationsSettings>();

        if (settings?.Enabled == true)
        {
            application.UseMiddleware<ValidationsMiddleware>();
        }

        return application;
    }
}
