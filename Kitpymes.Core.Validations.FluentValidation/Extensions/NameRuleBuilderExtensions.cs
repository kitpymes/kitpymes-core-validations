// -----------------------------------------------------------------------
// <copyright file="NameRuleBuilderExtensions.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations.FluentValidation.Extensions;

using global::FluentValidation;
using Kitpymes.Core.Shared.Extensions;
using Kitpymes.Core.Shared.Util;

/*
    Clase de extensión NameRuleBuilderExtensions
    Contiene las extensiones de las validaciones Name
*/

/// <summary>
/// Clase de extensión <c>NameRuleBuilderExtensions</c>.
/// Contiene las extensiones de las validaciones Name.
/// </summary>
/// <remarks>
/// <para>En esta clase se pueden agregar todas las validaciones Name.</para>
/// </remarks>
public static class NameRuleBuilderExtensions
{
    /// <summary>
    /// Comprueba si el valor ingresado es valido.
    /// </summary>
    /// <typeparam name="T">Entidad a validar.</typeparam>
    /// <param name="ruleBuilder">Regla de validación.</param>
    /// <param name="fieldName">Nombre del campo.</param>
    /// <returns>IRuleBuilder.</returns>
    public static IRuleBuilder<T, string?> IsName<T>(this IRuleBuilder<T, string?> ruleBuilder, string? fieldName = null)
    => ruleBuilder.Custom((value, context) =>
    {
        if (!value.ToIsName())
        {
            context.AddFailure(Messages.Name(value, fieldName ?? context.DisplayName));
        }
    });

    /// <summary>
    /// Comprueba si el valor ingresado es valido.
    /// </summary>
    /// <typeparam name="T">Entidad a validar.</typeparam>
    /// <param name="ruleBuilder">Regla de validación.</param>
    /// <param name="message">Mensaje a mostrar.</param>
    /// <returns>IRuleBuilder.</returns>
    public static IRuleBuilder<T, string?> IsNameWithMessage<T>(this IRuleBuilder<T, string?> ruleBuilder, string message)
    => ruleBuilder.Custom((value, context) =>
    {
        if (!value.ToIsName())
        {
            context.AddFailure(message);
        }
    });
}