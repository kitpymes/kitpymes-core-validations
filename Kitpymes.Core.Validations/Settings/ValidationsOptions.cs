// -----------------------------------------------------------------------
// <copyright file="ValidationsOptions.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations.Settings;

using System;
using System.Collections.Generic;
using System.Reflection;

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
    private readonly List<Assembly> _assemblies = new ();

    /// <summary>
    /// Obtiene los ensamblados de los validadores, sin repetir y en el orden en que se indicaron.
    /// </summary>
    internal IReadOnlyList<Assembly> Assemblies => _assemblies;

    /// <summary>
    /// Registra los validadores (<see cref="Abstractions.IValidator{T}"/>) de los ensamblados, el paso de validación
    /// del mediador y su comprobación de arranque.
    /// </summary>
    /// <param name="assemblies">Ensamblados donde están los validadores.</param>
    /// <returns>La clase ValidationsOptions.</returns>
    /// <exception cref="ArgumentException">Si no se indica ningún ensamblado o alguno es nulo.</exception>
    public ValidationsOptions WithAssemblies(params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0)
        {
            throw new ArgumentException("Indique al menos un ensamblado con validadores.", nameof(assemblies));
        }

        foreach (var assembly in assemblies)
        {
            if (assembly is null)
            {
                throw new ArgumentException("La lista de ensamblados no puede contener nulos.", nameof(assemblies));
            }

            if (!_assemblies.Contains(assembly))
            {
                _assemblies.Add(assembly);
            }
        }

        return this;
    }
}
