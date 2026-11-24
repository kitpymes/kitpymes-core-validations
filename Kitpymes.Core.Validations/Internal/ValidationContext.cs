// -----------------------------------------------------------------------
// <copyright file="ValidationContext.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations.Internal;

using System.Collections.Generic;
using System.Threading;
using Kitpymes.Core.Results;

/// <summary>
/// Estado de una validación: los errores que se van encontrando y los objetos anidados que se están
/// validando, para no entrar dos veces en un ciclo ni pasar del límite de profundidad.
/// </summary>
internal sealed class ValidationContext
{
    internal const int MaxDepth = 32;

    private readonly HashSet<object> _visiting = new (ReferenceEqualityComparer.Instance);

    private int _depth;

    internal ValidationContext(CancellationToken cancellationToken) => CancellationToken = cancellationToken;

    internal List<FieldError> Errors { get; } = new ();

    internal CancellationToken CancellationToken { get; }

    internal void Root(object instance) => _visiting.Add(instance);

    internal void Add(string field, string code, string message) => Errors.Add(new FieldError(field, code, message));

    internal void Nested<TChild>(TChild value, Validator<TChild> validator, string path)
    {
        if (value is null || !_visiting.Add(value))
        {
            return;
        }

        try
        {
            if (_depth >= MaxDepth)
            {
                Add(path, ValidationErrorCodes.Depth, Messages.Depth(path, MaxDepth));
                return;
            }

            _depth++;

            try
            {
                validator.Collect(value, this, path);
            }
            finally
            {
                _depth--;
            }
        }
        finally
        {
            _visiting.Remove(value);
        }
    }
}
