// -----------------------------------------------------------------------
// <copyright file="NotEqualRuleBuilderExtensions.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations.FluentValidation.Extensions;

using global::FluentValidation;
using Kitpymes.Core.Shared.Extensions;
using Kitpymes.Core.Shared.Util;

/*
    Clase de extensión NotEqualRuleBuilderExtensions
    Contiene las extensiones de las validaciones NotEqual
*/

/// <summary>
/// Clase de extensión <c>NotEqualRuleBuilderExtensions</c>.
/// Contiene las extensiones de las validaciones NotEqual.
/// </summary>
/// <remarks>
/// <para>En esta clase se pueden agregar todas las validaciones NotEqual.</para>
/// </remarks>
public static class NotEqualRuleBuilderExtensions
{
    /// <summary>
    /// Comprueba si el valor ingresado es valido.
    /// </summary>
    /// <typeparam name="T">Entidad a validar.</typeparam>
    /// <typeparam name="TProperty">Propiedad de la entidad.</typeparam>
    /// <param name="ruleBuilder">Regla de validación.</param>
    /// <param name="valueCompare">Valor a comparar.</param>
    /// <param name="fieldName">Nombre del campo.</param>
    /// <param name="fieldNameCompare">Nombre del campo a comparar.</param>
    /// <returns>IRuleBuilder.</returns>
    public static IRuleBuilder<T, TProperty> IsNotEqual<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, TProperty valueCompare, string? fieldName = null, string? fieldNameCompare = null)
     => ruleBuilder.Custom((value, context) =>
     {
         if (value.ToIsNotEqual(valueCompare))
         {
             context.AddFailure(Messages.Equal(fieldName ?? context.DisplayName, fieldNameCompare));
         }
     });

    /// <summary>
    /// Comprueba si el valor ingresado es valido.
    /// </summary>
    /// <typeparam name="T">Entidad a validar.</typeparam>
    /// <typeparam name="TProperty">Propiedad de la entidad.</typeparam>
    /// <param name="ruleBuilder">Regla de validación.</param>
    /// <param name="valueCompare">Valor a comparar.</param>
    /// <param name="message">Mensaje a mostrar.</param>
    /// <returns>IRuleBuilder.</returns>
    public static IRuleBuilder<T, TProperty> IsNotEqualWithMessage<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, TProperty valueCompare, string message)
     => ruleBuilder.Custom((value, context) =>
     {
         if (value.ToIsNotEqual(valueCompare))
         {
             context.AddFailure(message);
         }
     });
}