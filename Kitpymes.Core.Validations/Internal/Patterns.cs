// -----------------------------------------------------------------------
// <copyright file="Patterns.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations.Internal;

using System;
using System.Text.RegularExpressions;

/// <summary>
/// Expresiones regulares copiadas de Kitpymes.Core.Shared 2.0.1 (<c>Regexp</c>), para no depender de Shared.
/// Los tests comparan con el original.
/// </summary>
internal static class Patterns
{
    internal const string Email = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$";

    internal const string Name = @"^[a-zA-Z áàäâãÁÀÄÂÃÅéèëêÉÈËÊíìïîÍÌÏÎóòöôõøÓÒÖÔÕúùüûÚÙÜÛçÇħñÑ]*$";

    internal const string Subdomain = @"^[-._a-zA-Z0-9]*$";

    internal static readonly TimeSpan Timeout = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Compila la expresión al configurar la regla: un patrón no válido lanza <see cref="ArgumentException"/> ahí y
    /// no al validar.
    /// </summary>
    /// <param name="pattern">Expresión regular.</param>
    /// <returns>La expresión compilada, con un tiempo máximo de un segundo.</returns>
    internal static Regex Create(string pattern) => new (pattern, RegexOptions.CultureInvariant, Timeout);
}
