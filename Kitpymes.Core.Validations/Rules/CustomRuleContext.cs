// -----------------------------------------------------------------------
// <copyright file="CustomRuleContext.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations;

using System;
using Kitpymes.Core.Validations.Internal;

/// <summary>
/// Permite a una regla <c>Custom</c> añadir uno o varios errores al campo que valida.
/// </summary>
public sealed class CustomRuleContext
{
    private readonly ValidationContext _context;

    internal CustomRuleContext(ValidationContext context, string field)
    {
        _context = context;
        Field = field;
    }

    /// <summary>
    /// Obtiene la ruta del campo que se valida, con el formato de Angular (<c>address.street</c>, <c>items.0.name</c>).
    /// </summary>
    public string Field { get; }

    /// <summary>
    /// Añade un error al campo.
    /// </summary>
    /// <param name="message">Mensaje legible.</param>
    /// <param name="code">Código propio (<c>Modulo.Motivo</c>); si no se indica, <see cref="ValidationErrorCodes.Condition"/>.</param>
    /// <exception cref="ArgumentException">Si el mensaje está vacío, o el código no tiene la forma correcta o usa el prefijo <c>Validations.</c>.</exception>
    public void AddError(string message, string? code = null)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("El mensaje del error no puede estar vacío.", nameof(message));
        }

        _context.Add(Field, code is null ? ValidationErrorCodes.Condition : ErrorCodeRules.Custom(code, nameof(code)), message);
    }
}
