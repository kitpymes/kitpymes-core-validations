// -----------------------------------------------------------------------
// <copyright file="IsGreater.cs" company="Kitpymes">
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
    /// <param name="min">Valor minimo permitido.</param>
    /// <param name="fieldName">Nombre del campo.</param>
    /// <returns>ValidatorRuleOptions.</returns>
    public ValidatorRuleOptions IsGreater(long min, string? fieldName = null)
        => IsGreaterWithMessage(min, Messages.Greater(min, fieldName));

    /// <summary>
    /// Comprueba si el valor ingresado es valido.
    /// </summary>
    /// <param name="min">Valor minimo permitido.</param>
    /// <param name="message">Mensaje de validación.</param>
    /// <returns>ValidatorRuleOptions.</returns>
    public ValidatorRuleOptions IsGreaterWithMessage(long min, string message)
    {
        ValidatorRule.Add(() => !_value.ToIsNullOrEmpty() && _value.ToIsGreater(min), message);

        return this;
    }
}
