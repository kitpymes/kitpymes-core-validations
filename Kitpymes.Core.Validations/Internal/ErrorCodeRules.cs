// -----------------------------------------------------------------------
// <copyright file="ErrorCodeRules.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations.Internal;

using System;
using System.Linq;

/// <summary>
/// Comprueba los códigos de error propios con la misma forma que Kitpymes.Core.Results
/// (<c>Modulo.Motivo</c>, PascalCase, ASCII) y reserva el prefijo <c>Validations.</c> para la librería.
/// </summary>
internal static class ErrorCodeRules
{
    internal const string ReservedPrefix = "Validations.";

    internal static bool IsValid(string? code)
        => !string.IsNullOrEmpty(code) && code.Split('.').Length >= 2 && code.Split('.').All(IsPascalAscii);

    internal static string Custom(string code, string parameterName)
    {
        if (!IsValid(code))
        {
            throw new ArgumentException($"El código de error '{code}' no sigue la forma <Modulo>.<Motivo> (PascalCase, ASCII, sin espacios).", parameterName);
        }

        if (code.StartsWith(ReservedPrefix, StringComparison.Ordinal))
        {
            throw new ArgumentException($"El código de error '{code}' usa el prefijo '{ReservedPrefix}', que es de la librería.", parameterName);
        }

        return code;
    }

    private static bool IsPascalAscii(string part)
        => part.Length > 0
        && part[0] >= 'A' && part[0] <= 'Z'
        && part.All(c => (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'));
}
