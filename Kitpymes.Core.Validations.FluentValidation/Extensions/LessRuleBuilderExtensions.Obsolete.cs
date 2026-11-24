// -----------------------------------------------------------------------
// <copyright file="LessRuleBuilderExtensions.Obsolete.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations.FluentValidation.Extensions;

using System;
using System.ComponentModel;
using global::FluentValidation;
using Kitpymes.Core.Shared.Extensions;
using Kitpymes.Core.Shared.Util;

/*
    Clase de extensión LessRuleBuilderExtensions
    Contiene las extensiones de las validaciones Less
*/

/// <summary>
/// Clase de extensión <c>LessRuleBuilderExtensions</c>.
/// Contiene las extensiones de las validaciones Less.
/// </summary>
/// <remarks>
/// <para>En esta clase se pueden agregar todas las validaciones Less.</para>
/// </remarks>
[Obsolete("Extensión antigua de FluentValidation. Use las reglas propias de FluentValidation y el adaptador FluentValidationAdapter<T>, o Validator<T>. Se elimina en 3.0.0.", DiagnosticId = "KPY0008")]
[EditorBrowsable(EditorBrowsableState.Never)]
public static class LessRuleBuilderExtensions
{
    /// <summary>
    /// Comprueba si el valor ingresado es valido.
    /// </summary>
    /// <typeparam name="T">Entidad a validar.</typeparam>
    /// <typeparam name="TProperty">Propiedad de la entidad.</typeparam>
    /// <param name="ruleBuilder">Regla de validación.</param>
    /// <param name="min">Valor mínimo que puede tener el campo.</param>
    /// <param name="fieldName">Nombre del campo.</param>
    /// <returns>IRuleBuilder.</returns>
    public static IRuleBuilder<T, TProperty> IsLess<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, long min, string? fieldName = null)
    => ruleBuilder.Custom((value, context) =>
    {
        if (!value.ToIsNullOrEmpty() && value.ToIsLess(min))
        {
            context.AddFailure(Messages.Less(min, fieldName ?? context.DisplayName));
        }
    });

    /// <summary>
    /// Comprueba si el valor ingresado es valido.
    /// </summary>
    /// <typeparam name="T">Entidad a validar.</typeparam>
    /// <typeparam name="TProperty">Propiedad de la entidad.</typeparam>
    /// <param name="ruleBuilder">Regla de validación.</param>
    /// <param name="min">Valor mínimo que puede tener el campo.</param>
    /// <param name="message">Mensaje a mostrar.</param>
    /// <returns>IRuleBuilder.</returns>
    public static IRuleBuilder<T, TProperty> IsLessWithMessage<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, long min, string message)
    => ruleBuilder.Custom((value, context) =>
    {
        if (!value.ToIsNullOrEmpty() && value.ToIsLess(min))
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
    /// <param name="min">Valor mínimo que puede tener el campo.</param>
    /// <param name="fieldName">Nombre del campo.</param>
    /// <returns>IRuleBuilder.</returns>
    public static IRuleBuilder<T, TProperty> IsLessOrEqual<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, long min, string? fieldName = null)
    => ruleBuilder.Custom((value, context) =>
    {
        if (!value.ToIsNullOrEmpty() && value.ToIsLessOrEqual(min))
        {
            context.AddFailure(Messages.LessOrEqual(min, fieldName ?? context.DisplayName));
        }
    });

    /// <summary>
    /// Comprueba si el valor ingresado es valido.
    /// </summary>
    /// <typeparam name="T">Entidad a validar.</typeparam>
    /// <typeparam name="TProperty">Propiedad de la entidad.</typeparam>
    /// <param name="ruleBuilder">Regla de validación.</param>
    /// <param name="min">Valor mínimo que puede tener el campo.</param>
    /// <param name="message">Mensaje a mostrar.</param>
    /// <returns>IRuleBuilder.</returns>
    public static IRuleBuilder<T, TProperty> IsLessOrEqualWithMessage<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, long min, string message)
    => ruleBuilder.Custom((value, context) =>
    {
        if (!value.ToIsNullOrEmpty() && value.ToIsLessOrEqual(min))
        {
            context.AddFailure(message);
        }
    });
}