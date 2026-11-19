// -----------------------------------------------------------------------
// <copyright file="NullOrEmptyRuleBuilderExtensions.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations.FluentValidation.Extensions;

using global::FluentValidation;
using Kitpymes.Core.Shared.Extensions;
using Kitpymes.Core.Shared.Util;

/*
    Clase de extensión NullOrEmptyRuleBuilderExtensions
    Contiene las extensiones de las validaciones NullOrEmpty
*/

/// <summary>
/// Clase de extensión <c>NullOrEmptyRuleBuilderExtensions</c>.
/// Contiene las extensiones de las validaciones NullOrEmpty.
/// </summary>
/// <remarks>
/// <para>En esta clase se pueden agregar todas las validaciones NullOrEmpty.</para>
/// </remarks>
public static class NullOrEmptyRuleBuilderExtensions
{
    /// <summary>
    /// Comprueba si el valor ingresado es valido.
    /// </summary>
    /// <typeparam name="T">Entidad a validar.</typeparam>
    /// <typeparam name="TProperty">Propiedad de la entidad.</typeparam>
    /// <param name="ruleBuilder">Regla de validación.</param>
    /// <param name="fieldName">Nombre del campo.</param>
    /// <returns>IRuleBuilder.</returns>
    public static IRuleBuilder<T, TProperty> IsNullOrEmpty<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, string? fieldName = null)
    => ruleBuilder.Custom((value, context) =>
    {
        if (value.ToIsNullOrEmpty())
        {
            context.AddFailure(Messages.NullOrEmpty(fieldName ?? context.DisplayName));
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
    public static IRuleBuilder<T, TProperty> IsNullOrEmptyWithMessage<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, string message)
    => ruleBuilder.Custom((value, context) =>
    {
        if (value.ToIsNullOrEmpty())
        {
            context.AddFailure(message);
        }
    });
}