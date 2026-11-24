// -----------------------------------------------------------------------
// <copyright file="IsEqual.cs" company="Kitpymes">
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
    /// <param name="valueCompare">Valor a comparar.</param>
    /// <param name="fieldName">Nombre del campo.</param>
    /// <param name="fieldNameCompare">Nombre del campo a comparar.</param>
    /// <returns>ValidatorRuleOptions.</returns>
    public ValidatorRuleOptions IsEqual(object? valueCompare, string? fieldName = null, string? fieldNameCompare = null)
        => IsEqualWithMessage(valueCompare, Messages.Equal(fieldName, fieldNameCompare));

    /// <summary>
    /// Comprueba si el valor ingresado es valido.
    /// </summary>
    /// <param name="valueCompare">Valor a comparar.</param>
    /// <param name="message">Mensaje de error.</param>
    /// <returns>ValidatorRuleOptions.</returns>
    public ValidatorRuleOptions IsEqualWithMessage(object? valueCompare, string message)
    {
        AddError(() => _value.ToIsEqual(valueCompare), message.ToThrowIfNullOrEmpty());

        return this;
    }
}
