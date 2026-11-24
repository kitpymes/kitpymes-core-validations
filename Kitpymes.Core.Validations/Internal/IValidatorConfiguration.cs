// -----------------------------------------------------------------------
// <copyright file="IValidatorConfiguration.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations.Internal;

using System.Collections.Generic;

/// <summary>
/// Problemas de configuración de un validador que se ven al arrancar (por ejemplo, una regla sobre el
/// objeto entero sin <c>WithName</c>).
/// </summary>
internal interface IValidatorConfiguration
{
    IReadOnlyList<string> Problems();
}
