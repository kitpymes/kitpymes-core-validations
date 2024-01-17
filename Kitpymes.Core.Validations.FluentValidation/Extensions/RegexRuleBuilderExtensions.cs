// -----------------------------------------------------------------------
// <copyright file="RegexRuleBuilderExtensions.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations.FluentValidation.Extensions;

using global::FluentValidation;
using Kitpymes.Core.Shared.Extensions;
using Kitpymes.Core.Shared.Util;

/*
    Clase de extensión RegexRuleBuilderExtensions
    Contiene las extensiones de las validaciones Regex
*/

/// <summary>
/// Clase de extensión <c>RegexRuleBuilderExtensions</c>.
/// Contiene las extensiones de las validaciones Regex.
/// </summary>
/// <remarks>
/// <para>En esta clase se pueden agregar todas las validaciones Regex.</para>
/// </remarks>
public static class RegexRuleBuilderExtensions
{
    /// <summary>
    /// Comprueba si el valor ingresado es valido.
    /// </summary>
    /// <typeparam name="T">Entidad a validar.</typeparam>
    /// <param name="ruleBuilder">Regla de validación.</param>
    /// <param name="regex">Expresión regular que tiene que cumplir el campo.</param>
    /// <param name="fieldName">Nombre del campo.</param>
    /// <returns>IRuleBuilder.</returns>
    public static IRuleBuilder<T, string?> IsRegexNotMatch<T>(this IRuleBuilder<T, string?> ruleBuilder, string regex, string? fieldName = null)
    => ruleBuilder.Custom((value, context) =>
    {
        if (!value.ToIsNullOrEmpty() && !value.ToIsRegexMatch(regex))
        {
            context.AddFailure(Messages.Regex(fieldName ?? context.DisplayName));
        }
    });

    /// <summary>
    /// Comprueba si el valor ingresado es valido.
    /// </summary>
    /// <typeparam name="T">Entidad a validar.</typeparam>
    /// <param name="ruleBuilder">Regla de validación.</param>
    /// <param name="regex">Expresión regular que tiene que cumplir el campo.</param>
    /// <param name="message">Mensaje a mostrar.</param>
    /// <returns>IRuleBuilder.</returns>
    public static IRuleBuilder<T, string?> IsRegexNotMatchWithMessage<T>(this IRuleBuilder<T, string?> ruleBuilder, string regex, string message)
    => ruleBuilder.Custom((value, context) =>
    {
        if (!value.ToIsNullOrEmpty() && !value.ToIsRegexMatch(regex))
        {
            context.AddFailure(message);
        }
    });
}