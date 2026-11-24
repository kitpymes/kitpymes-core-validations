// -----------------------------------------------------------------------
// <copyright file="ValidationsApplicationBuilderExtensions.Obsolete.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations.Extensions;

using System;
using System.ComponentModel;
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
[Obsolete("API antigua. La validación va en el paso del mediador: LoadValidations(o => o.WithAssemblies(...)). Se elimina en 3.0.0.", DiagnosticId = "KPY0007")]
[EditorBrowsable(EditorBrowsableState.Never)]
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
