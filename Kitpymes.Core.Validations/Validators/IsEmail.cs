// -----------------------------------------------------------------------
// <copyright file="IsEmail.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations;

using Kitpymes.Core.Shared.Extensions;

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
    /// <param name="fieldName">Nombre del campo.</param>
    /// <returns>ValidatorRuleOptions.</returns>
    public ValidatorRuleOptions IsEmail(string? fieldName = null)
        => IsEmailWithMessage(Messages.Email(_value?.ToString(), fieldName));

    /// <summary>
    /// Comprueba si el valor ingresado es valido.
    /// </summary>
    /// <param name="message">Mensaje de validación.</param>
    /// <returns>ValidatorRuleOptions.</returns>
    public ValidatorRuleOptions IsEmailWithMessage(string message)
    {
        ValidatorRule.Add(() => _value is not string || !_value.ToString().ToIsEmail(), message.ToThrowIfNullOrEmpty());

        return this;
    }
}
