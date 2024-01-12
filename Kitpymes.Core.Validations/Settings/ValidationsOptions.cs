// -----------------------------------------------------------------------
// <copyright file="ValidationsOptions.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations.Settings;

/*
    Configuración de las validaciones ValidationsOptions
    Contiene las opciones de las validaciones
*/

/// <summary>
/// Configuración de las validaciones <c>ValidationsOptions</c>.
/// Contiene las opciones de las validaciones.
/// </summary>
/// <remarks>
/// <para>En esta clase se pueden agregar todas las opciones de las validaciones.</para>
/// </remarks>
public class ValidationsOptions
{
    /// <summary>
    /// Obtiene la configuración de las validaciones.
    /// </summary>
    public ValidationsSettings ValidationsSettings { get; private set; } = new ValidationsSettings();

    /// <summary>
    /// Si se habilita el servicio de validaciones.
    /// </summary>
    /// <param name="enabled">Si se habilita o no.</param>
    /// <returns>La clase ValidationsOptions.</returns>
    public ValidationsOptions WithEnabled(bool enabled = true)
    {
        ValidationsSettings.Enabled = enabled;

        return this;
    }
}
