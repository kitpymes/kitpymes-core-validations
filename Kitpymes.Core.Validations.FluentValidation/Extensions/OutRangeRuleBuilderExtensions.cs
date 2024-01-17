// -----------------------------------------------------------------------
// <copyright file="OutRangeRuleBuilderExtensions.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations.FluentValidation.Extensions;

using global::FluentValidation;
using Kitpymes.Core.Shared.Extensions;
using Kitpymes.Core.Shared.Util;

/*
    Clase de extensión OutRangeRuleBuilderExtensions
    Contiene las extensiones de las validaciones OutRange
*/

/// <summary>
/// Clase de extensión <c>OutRangeRuleBuilderExtensions</c>.
/// Contiene las extensiones de las validaciones OutRange.
/// </summary>
/// <remarks>
/// <para>En esta clase se pueden agregar todas las validaciones OutRange.</para>
/// </remarks>
public static class OutRangeRuleBuilderExtensions
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
    public static IRuleBuilder<T, TProperty> IsOutRange<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, long min, long max, string? fieldName = null)
     => ruleBuilder.Custom((value, context) =>
     {
         if (!value.ToIsNullOrEmpty() && value.ToIsOutRange(min, max))
         {
             context.AddFailure(Messages.OutRange(min, max, fieldName ?? context.DisplayName));
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
    public static IRuleBuilder<T, TProperty> IsOutRangeWithMessage<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, long min, long max, string message)
     => ruleBuilder.Custom((value, context) =>
     {
         if (!value.ToIsNullOrEmpty() && value.ToIsOutRange(min, max))
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
    public static IRuleBuilder<T, TProperty> IsOutOrEqualRange<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, long min, long max, string? fieldName = null)
     => ruleBuilder.Custom((value, context) =>
     {
         if (!value.ToIsNullOrEmpty() && value.ToIsOutOrEqualRange(min, max))
         {
             context.AddFailure(Messages.OutOrEqualRange(min, max, fieldName ?? context.DisplayName));
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
    public static IRuleBuilder<T, TProperty> IsOutOrEqualRangeWithMessage<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, long min, long max, string message)
     => ruleBuilder.Custom((value, context) =>
     {
         if (!value.ToIsNullOrEmpty() && value.ToIsOutOrEqualRange(min, max))
         {
             context.AddFailure(message);
         }
     });
}