// -----------------------------------------------------------------------
// <copyright file="IValidator{T}.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations.Abstractions;

using System.Threading;
using System.Threading.Tasks;
using Kitpymes.Core.Results;

/// <summary>
/// Valida la forma de un objeto (vacío, longitud, formato) y devuelve un <see cref="Result"/>.
/// </summary>
/// <remarks>
/// <para>Si el objeto no es válido, el resultado lleva un <see cref="Error"/> de tipo validación con un
/// <see cref="FieldError"/> por cada campo que falla. Las reglas de negocio (duplicados, saldos) no van aquí:
/// van en el dominio o en el manejador.</para>
/// <para>El <see cref="ValueTask{TResult}"/> se espera una sola vez y no se guarda.</para>
/// <para>Tiene el mismo nombre que <c>FluentValidation.IValidator&lt;T&gt;</c>: en un fichero que importe los dos
/// espacios de nombres, use un alias (<c>using KValidator = Kitpymes.Core.Validations.Abstractions;</c>).</para>
/// </remarks>
/// <typeparam name="T">Tipo del objeto que se valida.</typeparam>
public interface IValidator<in T>
{
    /// <summary>
    /// Valida el objeto.
    /// </summary>
    /// <param name="instance">Objeto a validar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Un resultado correcto, o un fallo de validación con el campo de cada error.</returns>
    ValueTask<Result> ValidateAsync(T instance, CancellationToken cancellationToken = default);
}
