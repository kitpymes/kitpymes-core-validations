// -----------------------------------------------------------------------
// <copyright file="IsLessOrEqual.cs" company="Kitpymes">
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
    /// <param name="min">Valor mínimo que puede tener.</param>
    /// <param name="fieldName">Nombre del campo.</param>
    /// <returns>ValidatorRuleOptions.</returns>
    public ValidatorRuleOptions IsLessOrEqual(long min, string? fieldName = null)
        => IsLessOrEqualWithMessage(min, Messages.LessOrEqual(min, fieldName));

    /// <summary>
    /// Comprueba si el valor ingresado es valido.
    /// </summary>
    /// <param name="min">Valor mínimo que puede tener.</param>
    /// <param name="message">Mensaje de error.</param>
    /// <returns>ValidatorRuleOptions.</returns>
    public ValidatorRuleOptions IsLessOrEqualWithMessage(long min, string message)
    {
        AddError(() => _value.ToIsLessOrEqual(min), message.ToThrowIfNullOrEmpty());

        return this;
    }
}
