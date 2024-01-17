// -----------------------------------------------------------------------
// <copyright file="IsNullOrAny.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations;

using System.Collections;
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
    /// <param name="fieldName">Nombre del campo.</param>
    /// <returns>ValidatorRuleOptions.</returns>
    public ValidatorRuleOptions IsNullOrAny(string? fieldName = null)
        => IsNullOrAnyWithMessage(Messages.NullOrAny(fieldName));

    /// <summary>
    /// Comprueba si el valor ingresado es valido.
    /// </summary>
    /// <param name="message">Mensaje de validación.</param>
    /// <returns>ValidatorRuleOptions.</returns>
    public ValidatorRuleOptions IsNullOrAnyWithMessage(string message)
    {
        ValidatorRule.Add(() => _value is not IEnumerable || (_value as IEnumerable).ToIsNullOrAny(), message.ToThrowIfNullOrEmpty());

        return this;
    }
}
