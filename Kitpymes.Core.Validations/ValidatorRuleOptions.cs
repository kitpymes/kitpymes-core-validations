// -----------------------------------------------------------------------
// <copyright file="ValidatorRuleOptions.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations;

using System;

using Kitpymes.Core.Shared.Extensions;

/*
    Configuración de los validadores ValidatorRuleOptions
    Contiene las opciones comunes de los validadores
*/

/// <summary>
/// Configuración de los validadores <c>ValidatorRuleOptions</c>.
/// Contiene las opciones comunes de los validadores.
/// </summary>
/// <remarks>
/// <para>En esta clase se pueden agregar todas las opciones comunes a todos los validadores.</para>
/// </remarks>
public partial class ValidatorRuleOptions
{
    private readonly object? _value;

    private readonly ValidatorRule? _rule;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ValidatorRuleOptions"/>.
    /// </summary>
    public ValidatorRuleOptions() { }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ValidatorRuleOptions"/>.
    /// </summary>
    /// <param name="value">El valor a validar.</param>
    public ValidatorRuleOptions(object? value) => _value = value;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ValidatorRuleOptions"/> que escribe en una cadena.
    /// </summary>
    /// <param name="value">El valor a validar.</param>
    /// <param name="rule">La cadena que recibe los errores.</param>
    internal ValidatorRuleOptions(object? value, ValidatorRule rule)
    {
        _value = value;
        _rule = rule;
    }

    private void AddError(Func<bool> condition, string message)
    {
        if (_rule is null)
        {
            ValidatorRule.Add(condition, message);
        }
        else
        {
            _rule.AddError(condition, message);
        }
    }
}
