// -----------------------------------------------------------------------
// <copyright file="ValidationExemption.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations.Pipeline;

using System;
using System.Linq;
using System.Reflection;

/// <summary>
/// Criterio de exención: un comando o una consulta sin datos de entrada no necesita validador.
/// </summary>
/// <remarks>
/// <para>Necesita validador si tiene alguna propiedad pública de instancia que se pueda leer (también heredada; sin
/// indexadores ni estáticas) o algún campo público de instancia. No hay atributo para saltárselo.</para>
/// </remarks>
internal static class ValidationExemption
{
    internal static bool NeedsValidator(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

        return type.GetProperties(flags).Any(p => p.GetMethod is not null && p.GetMethod.IsPublic && p.GetIndexParameters().Length == 0)
            || type.GetFields(flags).Length > 0;
    }
}
