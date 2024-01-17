// -----------------------------------------------------------------------
// <copyright file="GreaterRuleBuilderExtensions.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations.FluentValidation.Extensions;

using global::FluentValidation;
using Kitpymes.Core.Shared.Extensions;
using Kitpymes.Core.Shared.Util;

/*
    Clase de extensión GreaterRuleBuilderExtensions
    Contiene las extensiones de las validaciones Greater
*/

/// <summary>
/// Clase de extensión <c>GreaterRuleBuilderExtensions</c>.
/// Contiene las extensiones de las validaciones Greater.
/// </summary>
/// <remarks>
/// <para>En esta clase se pueden agregar todas las validaciones Greater.</para>
/// </remarks>
public static class GreaterRuleBuilderExtensions
{
    /// <summary>
    /// Comprueba si el valor ingresado es valido.
    /// </summary>
    /// <typeparam name="T">Entidad a validar.</typeparam>
    /// <typeparam name="TProperty">Propiedad de la entidad.</typeparam>
    /// <param name="ruleBuilder">Regla de validación.</param>
    /// <param name="max">Valor máximo que puede tener el campo.</param>
    /// <param name="fieldName">Nombre del campo.</param>
    /// <returns>IRuleBuilder.</returns>
    public static IRuleBuilder<T, TProperty> IsGreater<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, long max, string? fieldName = null)
    => ruleBuilder.Custom((value, context) =>
    {
        if (!value.ToIsNullOrEmpty() && value.ToIsGreater(max))
        {
            context.AddFailure(Messages.Greater(max, fieldName ?? context.DisplayName));
        }
    });

    /// <summary>
    /// Comprueba si el valor ingresado es valido.
    /// </summary>
    /// <typeparam name="T">Entidad a validar.</typeparam>
    /// <typeparam name="TProperty">Propiedad de la entidad.</typeparam>
    /// <param name="ruleBuilder">Regla de validación.</param>
    /// <param name="max">Valor máximo que puede tener el campo.</param>
    /// <param name="message">Mensaje a mostrar.</param>
    /// <returns>IRuleBuilder.</returns>
    public static IRuleBuilder<T, TProperty> IsGreaterWithMessage<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, long max, string message)
    => ruleBuilder.Custom((value, context) =>
    {
        if (!value.ToIsNullOrEmpty() && value.ToIsGreater(max))
        {
            context.AddFailure(message);
        }
    });

    /// <summary>
    /// Comprueba si el valor ingresado es valido.
    /// </summary>
    /// <typeparam name="T">Entidad a validar.</typeparam>
    /// <typeparam name="TProperty">Propiedad de la entidad.</typeparam>
    /// <param name="ruleBuilder">Regla de validación.</param>
    /// <param name="max">Valor máximo que puede tener el campo.</param>
    /// <param name="fieldName">Nombre del campo.</param>
    /// <returns>IRuleBuilder.</returns>
    public static IRuleBuilder<T, TProperty> IsGreaterOrEqual<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, long max, string? fieldName = null)
    => ruleBuilder.Custom((value, context) =>
    {
        if (!value.ToIsNullOrEmpty() && value.ToIsGreaterOrEqual(max))
        {
            context.AddFailure(Messages.GreaterOrEqual(max, fieldName ?? context.DisplayName));
        }
    });

    /// <summary>
    /// Comprueba si el valor ingresado es valido.
    /// </summary>
    /// <typeparam name="T">Entidad a validar.</typeparam>
    /// <typeparam name="TProperty">Propiedad de la entidad.</typeparam>
    /// <param name="ruleBuilder">Regla de validación.</param>
    /// <param name="max">Valor máximo que puede tener el campo.</param>
    /// <param name="message">Mensaje a mostrar.</param>
    /// <returns>IRuleBuilder.</returns>
    public static IRuleBuilder<T, TProperty> IsGreaterOrEqualWithMessage<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, long max, string message)
    => ruleBuilder.Custom((value, context) =>
    {
        if (!value.ToIsNullOrEmpty() && value.ToIsGreaterOrEqual(max))
        {
            context.AddFailure(message);
        }
    });
}