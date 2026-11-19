// -----------------------------------------------------------------------
// <copyright file="EmailRuleBuilderExtensions.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations.FluentValidation.Extensions;

using global::FluentValidation;
using Kitpymes.Core.Shared.Extensions;
using Kitpymes.Core.Shared.Util;

/*
    Clase de extensión EmailRuleBuilderExtensions
    Contiene las extensiones de las validaciones Email
*/

/// <summary>
/// Clase de extensión <c>EmailRuleBuilderExtensions</c>.
/// Contiene las extensiones de las validaciones Email.
/// </summary>
/// <remarks>
/// <para>En esta clase se pueden agregar todas las validaciones Email.</para>
/// </remarks>
public static class EmailRuleBuilderExtensions
{
    /// <summary>
    /// Comprueba si el valor ingresado es valido.
    /// </summary>
    /// <typeparam name="T">Entidad a validar.</typeparam>
    /// <param name="ruleBuilder">Regla de validación.</param>
    /// <param name="fieldName">Nombre del campo.</param>
    /// <returns>IRuleBuilder.</returns>
    public static IRuleBuilder<T, string?> IsEmail<T>(this IRuleBuilder<T, string?> ruleBuilder, string? fieldName = null)
    => ruleBuilder.Custom((value, context) =>
    {
        if (!value.ToIsEmail())
        {
            context.AddFailure(Messages.Email(value, fieldName ?? context.DisplayName));
        }
    });

    /// <summary>
    /// Comprueba si el valor ingresado es valido.
    /// </summary>
    /// <typeparam name="T">Entidad a validar.</typeparam>
    /// <param name="ruleBuilder">Regla de validación.</param>
    /// <param name="message">Mensaje a mostrar.</param>
    /// <returns>IRuleBuilder.</returns>
    public static IRuleBuilder<T, string?> IsEmailWithMessage<T>(this IRuleBuilder<T, string?> ruleBuilder, string message)
    => ruleBuilder.Custom((value, context) =>
    {
        if (!value.ToIsEmail())
        {
            context.AddFailure(message);
        }
    });
}