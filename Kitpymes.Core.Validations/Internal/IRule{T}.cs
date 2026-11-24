// -----------------------------------------------------------------------
// <copyright file="IRule{T}.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations.Internal;

/// <summary>
/// Una regla de un <see cref="Validator{T}"/>.
/// </summary>
/// <typeparam name="T">Tipo que se valida.</typeparam>
internal interface IRule<T>
{
    /// <summary>
    /// Valida la regla y añade sus errores al contexto.
    /// </summary>
    /// <param name="instance">Objeto que se valida.</param>
    /// <param name="context">Estado de la validación.</param>
    /// <param name="prefix">Ruta del objeto dentro del objeto raíz; vacía en la raíz.</param>
    void Validate(T instance, ValidationContext context, string prefix);

    /// <summary>
    /// Problema de configuración de la regla, si lo tiene.
    /// </summary>
    /// <param name="prefix">Ruta del objeto dentro del objeto raíz; vacía en la raíz.</param>
    /// <returns>El problema, o nulo si está bien configurada.</returns>
    string? Problem(string prefix);
}
