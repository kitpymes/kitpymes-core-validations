// -----------------------------------------------------------------------
// <copyright file="InsideRangeRuleBuilderExtensions.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations.FluentValidation.Extensions;

using global::FluentValidation;
using Kitpymes.Core.Shared.Extensions;
using Kitpymes.Core.Shared.Util;

/*
    Clase de extensión InsideRangeRuleBuilderExtensions
    Contiene las extensiones de las validaciones InsideRange
*/

/// <summary>
/// Clase de extensión <c>InsideRangeRuleBuilderExtensions</c>.
/// Contiene las extensiones de las validaciones InsideRange.
/// </summary>
/// <remarks>
/// <para>En esta clase se pueden agregar todas las validaciones InsideRange.</para>
/// </remarks>
public static class InsideRangeRuleBuilderExtensions
{
    /// <summary>
    /// Comprueba si el valor ingresado es valido.
    /// </summary>
    /// <typeparam name="T">Entidad a validar.</typeparam>
    /// <typeparam name="TProperty">Propiedad de la entidad.</typeparam>
    /// <param name="ruleBuilder">Regla de validación.</param>
    /// <param name="min">Valor mínimo que puede tener el campo.</param>
    /// <param name="max">Valor máximo que puede tener el campo.</param>
    /// <param name="fieldName">Nombre del campo.</param>
    /// <returns>IRuleBuilder.</returns>
    public static IRuleBuilder<T, TProperty> IsInsideRange<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, long min, long max, string? fieldName = null)
     => ruleBuilder.Custom((value, context) =>
     {
         if (!value.ToIsNullOrEmpty() && value.ToIsInsideRange(min, max))
         {
             context.AddFailure(Messages.InsideRange(min, max, fieldName ?? context.DisplayName));
         }
     });

    /// <summary>
    /// Comprueba si el valor ingresado es valido.
    /// </summary>
    /// <typeparam name="T">Entidad a validar.</typeparam>
    /// <typeparam name="TProperty">Propiedad de la entidad.</typeparam>
    /// <param name="ruleBuilder">Regla de validación.</param>
    /// <param name="min">Valor mínimo que puede tener el campo.</param>
    /// <param name="max">Valor máximo que puede tener el campo.</param>
    /// <param name="message">Mensaje a mostrar.</param>
    /// <returns>IRuleBuilder.</returns>
    public static IRuleBuilder<T, TProperty> IsInsideRangeWithMessage<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, long min, long max, string message)
     => ruleBuilder.Custom((value, context) =>
     {
         if (!value.ToIsNullOrEmpty() && value.ToIsInsideRange(min, max))
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
    /// <param name="max">Valor máximo que puede tener el campo.</param>
    /// <param name="fieldName">Nombre del campo.</param>
    /// <returns>IRuleBuilder.</returns>
    public static IRuleBuilder<T, TProperty> IsInsideOrEqualRange<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, long min, long max, string? fieldName = null)
     => ruleBuilder.Custom((value, context) =>
     {
         if (!value.ToIsNullOrEmpty() && value.ToIsInsideOrEqualRange(min, max))
         {
             context.AddFailure(Messages.InsideOrEqualRange(min, max, fieldName ?? context.DisplayName));
         }
     });

    /// <summary>
    /// Comprueba si el valor ingresado es valido.
    /// </summary>
    /// <typeparam name="T">Entidad a validar.</typeparam>
    /// <typeparam name="TProperty">Propiedad de la entidad.</typeparam>
    /// <param name="ruleBuilder">Regla de validación.</param>
    /// <param name="min">Valor mínimo que puede tener el campo.</param>
    /// <param name="max">Valor máximo que puede tener el campo.</param>
    /// <param name="message">Mensaje a mostrar.</param>
    /// <returns>IRuleBuilder.</returns>
    public static IRuleBuilder<T, TProperty> IsInsideOrEqualRangeWithMessage<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, long min, long max, string message)
     => ruleBuilder.Custom((value, context) =>
     {
         if (!value.ToIsNullOrEmpty() && value.ToIsInsideOrEqualRange(min, max))
         {
             context.AddFailure(message);
         }
     });
}