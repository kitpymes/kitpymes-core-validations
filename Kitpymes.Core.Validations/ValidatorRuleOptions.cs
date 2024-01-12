// -----------------------------------------------------------------------
// <copyright file="ValidatorRuleOptions.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations;

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

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ValidatorRuleOptions"/>.
    /// </summary>
    public ValidatorRuleOptions() { }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ValidatorRuleOptions"/>.
    /// </summary>
    /// <param name="value">El valor a validar.</param>
    public ValidatorRuleOptions(object? value) => _value = value;
}
