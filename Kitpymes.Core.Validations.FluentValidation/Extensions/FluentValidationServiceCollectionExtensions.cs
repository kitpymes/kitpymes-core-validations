// -----------------------------------------------------------------------
// <copyright file="FluentValidationServiceCollectionExtensions.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations.FluentValidation.Extensions;

using System.Linq;
using global::FluentValidation;
using global::FluentValidation.AspNetCore;
using Kitpymes.Core.Shared.Extensions;
using Kitpymes.Core.Validations.Abstractions.Exceptions;
using Kitpymes.Core.Validations.FluentValidation.Settings;
using Microsoft.Extensions.DependencyInjection;

/*
    Clase de extensión FluentValidationServiceCollectionExtensions
    Contiene las extensiones de los servicios de las validaciones
*/

/// <summary>
/// Clase de extensión <c>FluentValidationServiceCollectionExtensions</c>.
/// Contiene las extensiones de los servicios de las validaciones.
/// </summary>
/// <remarks>
/// <para>En esta clase se pueden agregar todas las extensiones de los servicios para las validaciones.</para>
/// </remarks>
public static class FluentValidationServiceCollectionExtensions
{
    /// <summary>
    /// Carga el servicio de validaciones.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <param name="settings">Configuración de FluentValidation.</param>
    /// <returns>La interface IServiceCollection.</returns>
    public static IServiceCollection LoadFluentValidation(this IServiceCollection services, FluentValidationSettings settings)
    {
        if (settings?.Enabled == true && settings.Assemblies?.Count > 0)
        {
            services.LoadFluentValidation([.. settings.Assemblies]);
        }

        return services;
    }

    /// <summary>
    /// Carga el servicio de validaciones.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <param name="assemblies">Ensamblados donde se aplica FluentValidation.</param>
    /// <returns>La interface IServiceCollection.</returns>
    public static IServiceCollection LoadFluentValidation(this IServiceCollection services, params string[] assemblies)
    {
        services
            .AddFluentValidationAutoValidation()
            .AddValidatorsFromAssemblies(assemblies.ToAssembly())
            .AddFluentValidationClientsideAdapters();

        var mvcBuilder = services.BuildServiceProvider().GetService<IMvcBuilder>() ?? services.AddMvc();

        mvcBuilder
            .ConfigureApiBehaviorOptions(x =>
            {
                x.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(e => e.Value?.Errors.Count > 0)
                        .ToDictionary(
                            key => key.Key,
                            value => value.Value!.Errors.Select(e => e.ErrorMessage));

                    throw new ValidationsException(errors);
                };
            });

        return services;
    }
}
