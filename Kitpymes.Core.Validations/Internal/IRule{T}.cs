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
    void Validate(T instance, ValidationContext context, string prefix);

    string? Problem(string prefix);
}
