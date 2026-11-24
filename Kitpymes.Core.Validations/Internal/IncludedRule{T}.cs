// -----------------------------------------------------------------------
// <copyright file="IncludedRule{T}.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations.Internal;

using System.Linq;

/// <summary>
/// Las reglas de otro validador del mismo tipo, añadidas con <c>Include</c>.
/// </summary>
/// <typeparam name="T">Tipo que se valida.</typeparam>
internal sealed class IncludedRule<T> : IRule<T>
{
    private readonly Validator<T> _validator;

    internal IncludedRule(Validator<T> validator) => _validator = validator;

    /// <inheritdoc/>
    public void Validate(T instance, ValidationContext context, string prefix) => _validator.Collect(instance, context, prefix);

    /// <inheritdoc/>
    public string? Problem(string prefix) => ((IValidatorConfiguration)_validator).Problems().FirstOrDefault();
}
