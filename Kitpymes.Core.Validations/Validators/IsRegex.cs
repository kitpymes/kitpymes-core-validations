// -----------------------------------------------------------------------
// <copyright file="IsRegex.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations;

using Kitpymes.Core.Shared.Extensions;
using Kitpymes.Core.Shared.Util;

/*
    Configuración de los validadores ValidatorRuleOptions
    Contiene las opciones de los validadores
*/

/// <summary>
/// Configuración de los validadores <c>ValidatorRuleOptions</c>.
/// Contiene las opciones de los validadores.
/// </summary>
/// <remarks>
/// <para>En esta clase se pueden agregar todas las opciones que necesitamos para el validador.</para>
/// </remarks>
public partial class ValidatorRuleOptions
{
    /// <summary>
    /// Comprueba si el valor ingresado es valido.
    /// </summary>
    /// <param name="regex">Expresión regular a validar.</param>
    /// <param name="fieldName">Nombre del campo.</param>
    /// <returns>ValidatorRuleOptions.</returns>
    public ValidatorRuleOptions IsRegex(string regex, string? fieldName = null)
        => IsRegexWithMessage(regex, Messages.Regex(fieldName));

    /// <summary>
    /// Comprueba si el valor ingresado es valido.
    /// </summary>
    /// <param name="regex">Expresión regular a validar.</param>
    /// <param name="message">Mensaje de error.</param>
    /// <returns>ValidatorRuleOptions.</returns>
    public ValidatorRuleOptions IsRegexWithMessage(string regex, string message)
    {
        AddError(() => _value is not string || !_value.ToString().ToIsRegexMatch(regex), message.ToThrowIfNullOrEmpty());

        return this;
    }
}
