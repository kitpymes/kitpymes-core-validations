// -----------------------------------------------------------------------
// <copyright file="NullOrAnyRuleBuilderExtensions.Obsolete.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations.FluentValidation.Extensions;

using System;
using System.Collections;
using System.ComponentModel;
using global::FluentValidation;
using Kitpymes.Core.Shared.Extensions;
using Kitpymes.Core.Shared.Util;

/*
    Clase de extensión NullOrAnyRuleBuilderExtensions
    Contiene las extensiones de las validaciones NullOrAny
*/

/// <summary>
/// Clase de extensión <c>NullOrAnyRuleBuilderExtensions</c>.
/// Contiene las extensiones de las validaciones NullOrAny.
/// </summary>
/// <remarks>
/// <para>En esta clase se pueden agregar todas las validaciones NullOrAny.</para>
/// </remarks>
[Obsolete("Extensión antigua de FluentValidation. Use las reglas propias de FluentValidation y el adaptador FluentValidationAdapter<T>, o Validator<T>. Se elimina en 3.0.0.", DiagnosticId = "KPY0008")]
[EditorBrowsable(EditorBrowsableState.Never)]
public static class NullOrAnyRuleBuilderExtensions
{
    /// <summary>
    /// Comprueba si el valor ingresado es valido.
    /// </summary>
    /// <typeparam name="T">Entidad a validar.</typeparam>
    /// <typeparam name="TProperty">Propiedad de la entidad.</typeparam>
    /// <param name="ruleBuilder">Regla de validación.</param>
    /// <param name="fieldName">Nombre del campo.</param>
    /// <returns>IRuleBuilder.</returns>
    public static IRuleBuilder<T, TProperty> IsNullOrAny<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, string? fieldName = null)
        where TProperty : IEnumerable?
    => ruleBuilder.Custom((value, context) =>
    {
        if (value is not IEnumerable || value.ToIsNullOrAny())
        {
            context.AddFailure(Messages.NullOrAny(fieldName ?? context.DisplayName));
        }
    });

    /// <summary>
    /// Comprueba si el valor ingresado es valido.
    /// </summary>
    /// <typeparam name="T">Entidad a validar.</typeparam>
    /// <typeparam name="TProperty">Propiedad de la entidad.</typeparam>
    /// <param name="ruleBuilder">Regla de validación.</param>
    /// <param name="message">Mensaje a mostrar.</param>
    /// <returns>IRuleBuilder.</returns>
    public static IRuleBuilder<T, TProperty> IsNullOrAnyWithMessage<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, string message)
       where TProperty : IEnumerable?
       => ruleBuilder.Custom((value, context) =>
       {
           if (value is not IEnumerable || value.ToIsNullOrAny())
           {
               context.AddFailure(message);
           }
       });
}