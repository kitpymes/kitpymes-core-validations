// -----------------------------------------------------------------------
// <copyright file="IsRange.cs" company="Kitpymes">
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
    /// <param name="max">Valor máximo que puede tener.</param>
    /// <param name="fieldName">Nombre del campo.</param>
    /// <returns>ValidatorRuleOptions.</returns>
    public ValidatorRuleOptions IsInsideRange(long min, long max, string? fieldName = null)
        => IsInsideRangeWithMessage(min, max, Messages.InsideRange(min, max, fieldName));

    /// <summary>
    /// Comprueba si el valor ingresado es valido.
    /// </summary>
    /// <param name="min">Valor mínimo que puede tener.</param>
    /// <param name="max">Valor máximo que puede tener.</param>
    /// <param name="message">Mensaje de error.</param>
    /// <returns>ValidatorRuleOptions.</returns>
    public ValidatorRuleOptions IsInsideRangeWithMessage(long min, long max, string message)
    {
        AddError(() => _value.ToIsInsideRange(min, max), message.ToThrowIfNullOrEmpty());

        return this;
    }

    /// <summary>
    /// Comprueba si el valor ingresado es valido.
    /// </summary>
    /// <param name="min">Valor mínimo que puede tener.</param>
    /// <param name="max">Valor máximo que puede tener.</param>
    /// <param name="fieldName">Nombre del campo.</param>
    /// <returns>ValidatorRuleOptions.</returns>
    public ValidatorRuleOptions IsInsideOrEqualRange(long min, long max, string? fieldName = null)
        => IsInsideOrEqualRangeWithMessage(min, max, Messages.InsideOrEqualRange(min, max, fieldName));

    /// <summary>
    /// Comprueba si el valor ingresado es valido.
    /// </summary>
    /// <param name="min">Valor mínimo que puede tener.</param>
    /// <param name="max">Valor máximo que puede tener.</param>
    /// <param name="message">Mensaje de error.</param>
    /// <returns>ValidatorRuleOptions.</returns>
    public ValidatorRuleOptions IsInsideOrEqualRangeWithMessage(long min, long max, string message)
    {
        AddError(() => _value.ToIsInsideOrEqualRange(min, max), message.ToThrowIfNullOrEmpty());

        return this;
    }

    /// <summary>
    /// Comprueba si el valor ingresado es valido.
    /// </summary>
    /// <param name="min">Valor mínimo que puede tener.</param>
    /// <param name="max">Valor máximo que puede tener.</param>
    /// <param name="fieldName">Nombre del campo.</param>
    /// <returns>ValidatorRuleOptions.</returns>
    public ValidatorRuleOptions IsOutRange(long min, long max, string? fieldName = null)
        => IsOutRangeWithMessage(min, max, Messages.OutRange(min, max, fieldName));

    /// <summary>
    /// Comprueba si el valor ingresado es valido.
    /// </summary>
    /// <param name="min">Valor mínimo que puede tener.</param>
    /// <param name="max">Valor máximo que puede tener.</param>
    /// <param name="message">Mensaje de error.</param>
    /// <returns>ValidatorRuleOptions.</returns>
    public ValidatorRuleOptions IsOutRangeWithMessage(long min, long max, string message)
    {
        AddError(() => _value.ToIsOutRange(min, max), message.ToThrowIfNullOrEmpty());

        return this;
    }

    /// <summary>
    /// Comprueba si el valor ingresado es valido.
    /// </summary>
    /// <param name="min">Valor mínimo que puede tener.</param>
    /// <param name="max">Valor máximo que puede tener.</param>
    /// <param name="fieldName">Nombre del campo.</param>
    /// <returns>ValidatorRuleOptions.</returns>
    public ValidatorRuleOptions IsOutOrEqualRange(long min, long max, string? fieldName = null)
        => IsOutOrEqualRangeWithMessage(min, max, Messages.OutOrEqualRange(min, max, fieldName));

    /// <summary>
    /// Comprueba si el valor ingresado es valido.
    /// </summary>
    /// <param name="min">Valor mínimo que puede tener.</param>
    /// <param name="max">Valor máximo que puede tener.</param>
    /// <param name="message">Mensaje de error.</param>
    /// <returns>ValidatorRuleOptions.</returns>
    public ValidatorRuleOptions IsOutOrEqualRangeWithMessage(long min, long max, string message)
    {
        AddError(() => _value.ToIsOutOrEqualRange(min, max), message.ToThrowIfNullOrEmpty());

        return this;
    }
}
