// -----------------------------------------------------------------------
// <copyright file="Validator.Obsolete.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations;

using System;
using System.ComponentModel;

/*
    Configuración de las opciones comunes de los validadores Validator
    Contiene las opciones comunes de los validadores
*/

/// <summary>
/// Configuración de las opciones comunes de los validadores <c>Validator</c>.
/// Contiene las opciones comunes de los validadores.
/// </summary>
/// <remarks>
/// <para>En esta clase se pueden agregar todas las opciones comunes que necesitamos para los validadores.</para>
/// </remarks>
[Obsolete("API antigua. Use Validator<T> e IValidator<T>, que devuelven un Result con un FieldError por campo. Se elimina en 3.0.0.", DiagnosticId = "KPY0005")]
[EditorBrowsable(EditorBrowsableState.Never)]
public static class Validator
{
    /// <summary>
    /// Para agregar una nueva regla de validación.
    /// </summary>
    /// <param name="value">Valor a validar.</param>
    /// <param name="options">Validadores disponibles.</param>
    /// <returns>Una cadena nueva, con sus propios errores.</returns>
    public static ValidatorRule AddRule(object? value, Action<ValidatorRuleOptions> options)
    {
        return new ValidatorRule().AddRule(value, options);
    }

    /// <summary>
    /// Para agregar una nueva regla de validación.
    /// </summary>
    /// <param name="condition">Condición a validar.</param>
    /// <param name="message">Mensaje de error a mostrar.</param>
    /// <returns>Una cadena nueva, con sus propios errores.</returns>
    public static ValidatorRule AddRule(Func<bool> condition, string message)
    {
        return new ValidatorRule().AddRule(condition, message);
    }
}
