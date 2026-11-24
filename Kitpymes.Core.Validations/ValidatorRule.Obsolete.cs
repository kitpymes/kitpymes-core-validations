// -----------------------------------------------------------------------
// <copyright file="ValidatorRule.Obsolete.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Kitpymes.Core.Shared.Extensions;
using Kitpymes.Core.Shared.Util;
using Kitpymes.Core.Validations.Abstractions.Exceptions;

/*
    Configuración de las reglas de validación ValidatorRule
    Contiene las reglas de validación
*/

/// <summary>
/// Configuración de las reglas de validación <c>ValidatorRule</c>.
/// Contiene las reglas de validación de una cadena.
/// </summary>
/// <remarks>
/// <para>Cada cadena que empieza en <see cref="Validator"/> tiene sus propios errores: dos cadenas,
/// aunque se ejecuten a la vez en hilos distintos, no comparten nada.</para>
/// </remarks>
[Obsolete("API antigua. Use Validator<T> e IValidator<T>, que devuelven un Result con un FieldError por campo. Se elimina en 3.0.0.", DiagnosticId = "KPY0005")]
[EditorBrowsable(EditorBrowsableState.Never)]
public class ValidatorRule
{
    [ThreadStatic]
    private static ValidatorRule? _current;

    private readonly List<string> _errors = [];

    private bool _stopFirstError;

    /// <summary>
    /// Obtiene un valor que indica si la cadena no tiene ningún error.
    /// </summary>
    /// <returns>Un booleano.</returns>
    public bool IsValid
    {
        get
        {
            lock (_errors)
            {
                return _errors.Count == 0;
            }
        }
    }

    /// <summary>
    /// Agrega un error a la cadena que se está configurando si la condición es verdadera.
    /// </summary>
    /// <remarks>
    /// <para>Sólo se puede llamar desde las opciones de una regla, dentro de <c>AddRule(valor, opciones)</c>, y en el
    /// mismo hilo y antes de que terminen. Las reglas de <see cref="ValidatorRuleOptions"/> no pasan por aquí: escriben
    /// directamente en su cadena.</para>
    /// </remarks>
    /// <param name="condition">Si la condicion es verdadera, se agrega el error.</param>
    /// <param name="message">Mensaje de validación.</param>
    /// <exception cref="InvalidOperationException">Si se llama fuera de las opciones de una regla.</exception>
    public static void Add(Func<bool> condition, string message)
    {
        var current = _current ?? throw new InvalidOperationException(
            "ValidatorRule.Add sólo se puede usar dentro de las opciones de una regla (Validator.AddRule(valor, opciones)).");

        current.AddError(condition, message);
    }

    /// <summary>
    /// Si quiere que pare de validar cuando encuentre el primer error: las reglas que se añadan después del
    /// primer error no se evalúan.
    /// </summary>
    /// <param name="stopFirstError">Párametro de seteo.</param>
    /// <returns>ValidatorRule.</returns>
    public ValidatorRule StopFirstError(bool stopFirstError = true)
    {
        _stopFirstError = stopFirstError;

        return this;
    }

    /// <summary>
    /// Para agregar una nueva regla de validación a esta cadena.
    /// </summary>
    /// <param name="value">Valor a validar.</param>
    /// <param name="options">Validadores disponibles.</param>
    /// <returns>Esta misma cadena.</returns>
    public ValidatorRule AddRule(object? value, Action<ValidatorRuleOptions> options)
    {
        var previous = _current;

        _current = this;

        try
        {
            options.ToConfigureOrDefault(new ValidatorRuleOptions(value, this));
        }
        finally
        {
            _current = previous;
        }

        return this;
    }

    /// <summary>
    /// Para agregar una nueva regla de validación a esta cadena.
    /// </summary>
    /// <param name="condition">Condición a validar.</param>
    /// <param name="message">Mensaje de error a mostrar.</param>
    /// <returns>Esta misma cadena.</returns>
    public ValidatorRule AddRule(Func<bool> condition, string message)
    {
        AddError(() => condition is not null && condition(), message.ToThrowIfNullOrEmpty());

        return this;
    }

    /// <summary>
    /// Lanza una excepción del tipo ValidationsException si es que hubo algún error.
    /// </summary>
    public void Throw() => Throw<ValidationsException>();

    /// <summary>
    /// Lanza una excepción asincrona del tipo ValidationsException si es que hubo algún error.
    /// </summary>
    /// <returns>Task.</returns>
    public Task ThrowAsync() => ThrowAsync<ValidationsException>();

    /// <summary>
    /// Lanza una excepción asincrona del tipo <typeparamref name="TException"/> si es que hubo algún error.
    /// </summary>
    /// <remarks>
    /// <para>Se ejecuta en el mismo hilo; el error llega en la tarea devuelta, como antes.</para>
    /// </remarks>
    /// <typeparam name="TException">Tipo de la excepción.</typeparam>
    /// <returns>Task.</returns>
    public async Task ThrowAsync<TException>()
        where TException : Exception
    {
        Throw<TException>();

        await Task.CompletedTask.ConfigureAwait(false);
    }

    /// <summary>
    /// Lanza una excepción del tipo <typeparamref name="TException"/> si es que hubo algún error.
    /// </summary>
    /// <typeparam name="TException">Tipo de la excepción.</typeparam>
    public void Throw<TException>()
        where TException : Exception
    {
        string[] errors;

        lock (_errors)
        {
            if (_errors.Count == 0)
            {
                return;
            }

            errors = _stopFirstError
                 ? new string[] { _errors[0] }
                 : _errors.ToArray();

            _errors.Clear();
        }

        Verify.ToThrowIf<TException>(errors.ToString(", "), () => true);
    }

    /// <summary>
    /// Agrega el error si la condición es verdadera. Con <see cref="StopFirstError(bool)"/> y un error ya
    /// guardado, no evalúa la condición.
    /// </summary>
    /// <param name="condition">Si la condicion es verdadera, se agrega el error.</param>
    /// <param name="message">Mensaje de validación.</param>
    internal void AddError(Func<bool> condition, string message)
    {
        lock (_errors)
        {
            if (_stopFirstError && _errors.Count > 0)
            {
                return;
            }
        }

        if (condition())
        {
            lock (_errors)
            {
                _errors.Add(message);
            }
        }
    }
}
