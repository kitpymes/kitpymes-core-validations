// -----------------------------------------------------------------------
// <copyright file="EqualRuleBuilderExtensions.Obsolete.cs" company="Kitpymes">
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
    Clase de extensión EqualRuleBuilderExtensions
    Contiene las extensiones de las validaciones Equal
*/

/// <summary>
/// Clase de extensión <c>EqualRuleBuilderExtensions</c>.
/// Contiene las extensiones de las validaciones Equal.
/// </summary>
/// <remarks>
/// <para>En esta clase se pueden agregar todas las validaciones Equal.</para>
/// </remarks>
[Obsolete("Extensión antigua de FluentValidation. Use las reglas propias de FluentValidation y el adaptador FluentValidationAdapter<T>, o Validator<T>. Se elimina en 3.0.0.", DiagnosticId = "KPY0008")]
[EditorBrowsable(EditorBrowsableState.Never)]
public static class EqualRuleBuilderExtensions
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
    public static IRuleBuilder<T, TProperty> IsEqual<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, TProperty valueCompare, string? fieldName = null, string? fieldNameCompare = null)
     => ruleBuilder.Custom((value, context) =>
     {
         if (value.ToIsEqual(valueCompare))
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
    public static IRuleBuilder<T, TProperty> IsEqualWithMessage<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, TProperty valueCompare, string message)
     => ruleBuilder.Custom((value, context) =>
     {
         if (value.ToIsEqual(valueCompare))
         {
             context.AddFailure(message);
         }
     });
}