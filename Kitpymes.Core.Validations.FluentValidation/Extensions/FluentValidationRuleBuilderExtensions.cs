// -----------------------------------------------------------------------
// <copyright file="FluentValidationRuleBuilderExtensions.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations.FluentValidation.Extensions;

using global::FluentValidation;

/*
    Clase de extensión FluentValidationRuleBuilderExtensions
    Contiene las extensiones de los servicios de las validaciones
*/

/// <summary>
/// Clase de extensión <c>FluentValidationRuleBuilderExtensions</c>.
/// Contiene las extensiones de los servicios de las validaciones.
/// </summary>
/// <remarks>
/// <para>En esta clase se pueden agregar todas las extensiones de los servicios para las validaciones.</para>
/// </remarks>
public static class FluentValidationRuleBuilderExtensions
{
    /// <summary>
    /// Si queremos que pare de validar cuando ocurre el primer error.
    /// </summary>
    /// <typeparam name="TEntity">Tipo de entidad a validar.</typeparam>
    /// <typeparam name="TProperty">Tipo de propiedad a validar.</typeparam>
    /// <param name="ruleBuilder">Regla de validacion.</param>
    /// <param name="stop">Si habilitamos la validacion para que pare cuando ocurre el primer error.</param>
    /// <returns>IRuleBuilder{TEntity, TProperty}.</returns>
    public static IRuleBuilder<TEntity, TProperty> StopFirstError<TEntity, TProperty>(this IRuleBuilderInitial<TEntity, TProperty> ruleBuilder, bool stop = true)
        => stop ? ruleBuilder.Cascade(CascadeMode.Stop) : ruleBuilder;

    /// <summary>
    /// Sobreescribe el nombre de la propiedad.
    /// </summary>
    /// <typeparam name="TEntity">Tipo de entidad a validar.</typeparam>
    /// <typeparam name="TProperty">Tipo de propiedad a validar.</typeparam>
    /// <param name="ruleBuilder">Regla de validacion.</param>
    /// <param name="propertyName">Nombre de la propiedad.</param>
    public static void WithOverridePropertyName<TEntity, TProperty>(this IRuleBuilder<TEntity, TProperty> ruleBuilder, string propertyName)
        => ((IRuleBuilderInitial<TEntity, TProperty>)ruleBuilder).Configure(x => x.PropertyName = propertyName);
}
