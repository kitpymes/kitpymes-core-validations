// -----------------------------------------------------------------------
// <copyright file="Messages.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations.Internal;

using System;
using System.Globalization;

/// <summary>
/// Mensajes por defecto de las reglas. Se resuelven al validar, con la cultura de ese momento:
/// inglés si la cultura es <c>en</c>; si no, español (la misma regla que Kitpymes.Core.Results).
/// </summary>
internal static class Messages
{
    internal static bool English => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "en";

    internal static string Invalid()
        => English ? "There are validation errors." : "Hay errores de validación.";

    internal static string NullInstance(string type)
        => English ? $"The object to validate ({type}) is null." : $"El objeto a validar ({type}) es nulo.";

    internal static string Depth(string field, int max)
        => English
            ? $"The field '{field}' is nested more than {max} levels deep."
            : $"El campo '{field}' está anidado a más de {max} niveles.";

    internal static string For(string code, string field, object? first = null, object? second = null)
    {
        var en = English;
        var a = Format(first, en);
        var b = Format(second, en);

        return code switch
        {
            ValidationErrorCodes.Required => en ? $"The field '{field}' is required." : $"El campo '{field}' es obligatorio.",
            ValidationErrorCodes.MaxLength => en ? $"The field '{field}' must have at most {a} characters." : $"El campo '{field}' debe tener como máximo {a} caracteres.",
            ValidationErrorCodes.MinLength => en ? $"The field '{field}' must have at least {a} characters." : $"El campo '{field}' debe tener como mínimo {a} caracteres.",
            ValidationErrorCodes.Email => en ? $"The field '{field}' is not a valid email." : $"El campo '{field}' no es un email válido.",
            ValidationErrorCodes.Name => en ? $"The field '{field}' is not a valid name." : $"El campo '{field}' no es un nombre válido.",
            ValidationErrorCodes.Subdomain => en ? $"The field '{field}' is not a valid subdomain." : $"El campo '{field}' no es un subdominio válido.",
            ValidationErrorCodes.Format => en ? $"The field '{field}' does not have a valid format." : $"El campo '{field}' no tiene un formato válido.",
            ValidationErrorCodes.GreaterThan => en ? $"The field '{field}' must be greater than {a}." : $"El campo '{field}' debe ser mayor que {a}.",
            ValidationErrorCodes.GreaterThanOrEqual => en ? $"The field '{field}' must be greater than or equal to {a}." : $"El campo '{field}' debe ser mayor o igual que {a}.",
            ValidationErrorCodes.LessThan => en ? $"The field '{field}' must be less than {a}." : $"El campo '{field}' debe ser menor que {a}.",
            ValidationErrorCodes.LessThanOrEqual => en ? $"The field '{field}' must be less than or equal to {a}." : $"El campo '{field}' debe ser menor o igual que {a}.",
            ValidationErrorCodes.Between => en ? $"The field '{field}' must be between {a} and {b}." : $"El campo '{field}' debe estar entre {a} y {b}.",
            ValidationErrorCodes.Equal => en ? $"The field '{field}' must be {a}." : $"El campo '{field}' debe ser {a}.",
            ValidationErrorCodes.NotEqual => en ? $"The field '{field}' cannot be {a}." : $"El campo '{field}' no puede ser {a}.",
            ValidationErrorCodes.Enum => en ? $"The field '{field}' has a value that is not valid." : $"El campo '{field}' tiene un valor que no es válido.",
            _ => en ? $"The field '{field}' is not valid." : $"El campo '{field}' no es válido.",
        };
    }

    // Los valores se escriben con la cultura de los mensajes (CurrentUICulture), no con CurrentCulture, para que
    // un mensaje en inglés no lleve "1,5"; un valor nulo se escribe como tal.
    private static string Format(object? value, bool english)
        => value switch
        {
            null => english ? "null" : "nulo",
            IFormattable formattable => formattable.ToString(null, CultureInfo.CurrentUICulture),
            _ => value.ToString() ?? string.Empty,
        };
}
