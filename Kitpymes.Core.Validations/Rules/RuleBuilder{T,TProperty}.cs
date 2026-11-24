// -----------------------------------------------------------------------
// <copyright file="RuleBuilder{T,TProperty}.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kitpymes.Core.Validations.Internal;

/// <summary>
/// Reglas de un campo de un <see cref="Validator{T}"/>, o de cada elemento de una colección.
/// </summary>
/// <remarks>
/// <para>Por defecto, en cuanto una regla falla no se evalúan las siguientes del mismo campo
/// (<see cref="ContinueOnError"/> para no parar). <see cref="WithMessage"/> y <see cref="WithErrorCode"/>
/// cambian la última regla añadida. Salvo <see cref="Required"/>, <see cref="Must(Func{TProperty, bool})"/>,
/// <see cref="Custom"/>, <see cref="Equal"/> y <see cref="NotEqual"/>, las reglas no se evalúan si el valor es nulo:
/// para eso está <see cref="Required"/>.</para>
/// </remarks>
/// <typeparam name="T">Tipo que se valida.</typeparam>
/// <typeparam name="TProperty">Tipo del campo.</typeparam>
public sealed class RuleBuilder<T, TProperty> : IRule<T>
{
    private readonly MemberPath _path;

    private readonly bool _each;

    private readonly List<Check> _checks = new ();

    private Func<T, bool>? _when;

    private string? _name;

    private bool _continueOnError;

    private Action<object, ValidationContext, string>? _child;

    internal RuleBuilder(MemberPath path, bool each)
    {
        _path = path;
        _each = each;
    }

    /// <summary>
    /// El campo es obligatorio: no puede ser nulo, un texto vacío o en blanco, ni una colección vacía.
    /// </summary>
    /// <returns>Este mismo constructor de reglas.</returns>
    public RuleBuilder<T, TProperty> Required()
        => Add(ValidationErrorCodes.Required, skipNull: false, (v, _) => !IsEmpty(v), f => Messages.For(ValidationErrorCodes.Required, f));

    /// <summary>
    /// El texto tiene como máximo <paramref name="max"/> caracteres.
    /// </summary>
    /// <param name="max">Longitud máxima.</param>
    /// <returns>Este mismo constructor de reglas.</returns>
    public RuleBuilder<T, TProperty> MaxLength(int max)
    {
        RequireString(nameof(MaxLength));
        ArgumentOutOfRangeException.ThrowIfNegative(max);
        return Add(ValidationErrorCodes.MaxLength, skipNull: true, (v, _) => ((string)(object)v!).Length <= max, f => Messages.For(ValidationErrorCodes.MaxLength, f, max));
    }

    /// <summary>
    /// El texto tiene como mínimo <paramref name="min"/> caracteres.
    /// </summary>
    /// <param name="min">Longitud mínima.</param>
    /// <returns>Este mismo constructor de reglas.</returns>
    public RuleBuilder<T, TProperty> MinLength(int min)
    {
        RequireString(nameof(MinLength));
        ArgumentOutOfRangeException.ThrowIfNegative(min);
        return Add(ValidationErrorCodes.MinLength, skipNull: true, (v, _) => ((string)(object)v!).Length >= min, f => Messages.For(ValidationErrorCodes.MinLength, f, min));
    }

    /// <summary>
    /// El texto tiene un formato de email válido.
    /// </summary>
    /// <returns>Este mismo constructor de reglas.</returns>
    public RuleBuilder<T, TProperty> Email() => Pattern(nameof(Email), ValidationErrorCodes.Email, Patterns.Email);

    /// <summary>
    /// El texto sólo tiene letras (también con tilde) y espacios.
    /// </summary>
    /// <returns>Este mismo constructor de reglas.</returns>
    public RuleBuilder<T, TProperty> Name() => Pattern(nameof(Name), ValidationErrorCodes.Name, Patterns.Name);

    /// <summary>
    /// El texto sólo tiene letras sin tilde, números, puntos, guiones y guiones bajos.
    /// </summary>
    /// <returns>Este mismo constructor de reglas.</returns>
    public RuleBuilder<T, TProperty> Subdomain() => Pattern(nameof(Subdomain), ValidationErrorCodes.Subdomain, Patterns.Subdomain);

    /// <summary>
    /// El texto cumple la expresión regular.
    /// </summary>
    /// <param name="pattern">Expresión regular.</param>
    /// <returns>Este mismo constructor de reglas.</returns>
    public RuleBuilder<T, TProperty> Matches(string pattern)
    {
        ArgumentException.ThrowIfNullOrEmpty(pattern);
        return Pattern(nameof(Matches), ValidationErrorCodes.Format, pattern);
    }

    /// <summary>
    /// El valor es mayor que <paramref name="limit"/>.
    /// </summary>
    /// <param name="limit">Límite.</param>
    /// <returns>Este mismo constructor de reglas.</returns>
    public RuleBuilder<T, TProperty> GreaterThan(TProperty limit) => Compare(nameof(GreaterThan), ValidationErrorCodes.GreaterThan, limit, c => c > 0);

    /// <summary>
    /// El valor es mayor o igual que <paramref name="limit"/>.
    /// </summary>
    /// <param name="limit">Límite.</param>
    /// <returns>Este mismo constructor de reglas.</returns>
    public RuleBuilder<T, TProperty> GreaterThanOrEqual(TProperty limit) => Compare(nameof(GreaterThanOrEqual), ValidationErrorCodes.GreaterThanOrEqual, limit, c => c >= 0);

    /// <summary>
    /// El valor es menor que <paramref name="limit"/>.
    /// </summary>
    /// <param name="limit">Límite.</param>
    /// <returns>Este mismo constructor de reglas.</returns>
    public RuleBuilder<T, TProperty> LessThan(TProperty limit) => Compare(nameof(LessThan), ValidationErrorCodes.LessThan, limit, c => c < 0);

    /// <summary>
    /// El valor es menor o igual que <paramref name="limit"/>.
    /// </summary>
    /// <param name="limit">Límite.</param>
    /// <returns>Este mismo constructor de reglas.</returns>
    public RuleBuilder<T, TProperty> LessThanOrEqual(TProperty limit) => Compare(nameof(LessThanOrEqual), ValidationErrorCodes.LessThanOrEqual, limit, c => c <= 0);

    /// <summary>
    /// El valor está entre <paramref name="min"/> y <paramref name="max"/>, los dos incluidos.
    /// </summary>
    /// <param name="min">Mínimo.</param>
    /// <param name="max">Máximo.</param>
    /// <returns>Este mismo constructor de reglas.</returns>
    public RuleBuilder<T, TProperty> Between(TProperty min, TProperty max)
    {
        RequireComparable(nameof(Between));
        var comparer = Comparer<TProperty>.Default;

        if (comparer.Compare(min, max) > 0)
        {
            throw new ArgumentException($"En Between el mínimo ({min}) no puede ser mayor que el máximo ({max}).", nameof(min));
        }

        return Add(
            ValidationErrorCodes.Between,
            skipNull: true,
            (v, _) => comparer.Compare(v, min) >= 0 && comparer.Compare(v, max) <= 0,
            f => Messages.For(ValidationErrorCodes.Between, f, min, max));
    }

    /// <summary>
    /// El valor es igual a <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Valor esperado.</param>
    /// <returns>Este mismo constructor de reglas.</returns>
    public RuleBuilder<T, TProperty> Equal(TProperty value)
        => Add(ValidationErrorCodes.Equal, skipNull: false, (v, _) => EqualityComparer<TProperty>.Default.Equals(v, value), f => Messages.For(ValidationErrorCodes.Equal, f, value));

    /// <summary>
    /// El valor es distinto de <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Valor que no se admite.</param>
    /// <returns>Este mismo constructor de reglas.</returns>
    public RuleBuilder<T, TProperty> NotEqual(TProperty value)
        => Add(ValidationErrorCodes.NotEqual, skipNull: false, (v, _) => !EqualityComparer<TProperty>.Default.Equals(v, value), f => Messages.For(ValidationErrorCodes.NotEqual, f, value));

    /// <summary>
    /// El valor está definido en el enumerado; con <see cref="FlagsAttribute"/>, todos sus bits están definidos.
    /// </summary>
    /// <returns>Este mismo constructor de reglas.</returns>
    public RuleBuilder<T, TProperty> IsInEnum()
    {
        var type = Nullable.GetUnderlyingType(typeof(TProperty)) ?? typeof(TProperty);

        if (!type.IsEnum)
        {
            throw new InvalidOperationException($"IsInEnum sólo se puede usar con enumerados, y el campo '{_path.Path}' es {typeof(TProperty).Name}.");
        }

        var flags = type.GetCustomAttribute<FlagsAttribute>() is not null;
        var mask = Enum.GetValues(type).Cast<object>().Aggregate(0UL, (all, value) => all | Convert.ToUInt64(value, System.Globalization.CultureInfo.InvariantCulture));

        return Add(
            ValidationErrorCodes.Enum,
            skipNull: true,
            (v, _) => Enum.IsDefined(type, v!) || (flags && (Convert.ToUInt64(v, System.Globalization.CultureInfo.InvariantCulture) & ~mask) == 0),
            f => Messages.For(ValidationErrorCodes.Enum, f));
    }

    /// <summary>
    /// El valor cumple la condición.
    /// </summary>
    /// <param name="predicate">Condición sobre el valor; se evalúa también si es nulo.</param>
    /// <returns>Este mismo constructor de reglas.</returns>
    public RuleBuilder<T, TProperty> Must(Func<TProperty, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return Must((_, v) => predicate(v));
    }

    /// <summary>
    /// El valor cumple la condición, que también puede mirar el objeto entero (reglas entre varios campos).
    /// </summary>
    /// <param name="predicate">Condición sobre el objeto y el valor; se evalúa también si el valor es nulo.</param>
    /// <returns>Este mismo constructor de reglas.</returns>
    public RuleBuilder<T, TProperty> Must(Func<T, TProperty, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return Add(ValidationErrorCodes.Condition, skipNull: false, (v, root) => predicate(root, v), f => Messages.For(ValidationErrorCodes.Condition, f));
    }

    /// <summary>
    /// Regla propia que puede añadir varios errores al campo (por ejemplo, cada requisito de una contraseña).
    /// </summary>
    /// <param name="rule">Regla; se ejecuta también si el valor es nulo.</param>
    /// <returns>Este mismo constructor de reglas.</returns>
    public RuleBuilder<T, TProperty> Custom(Action<TProperty, CustomRuleContext> rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        _checks.Add(new Check(ValidationErrorCodes.Condition, false, null, _ => string.Empty) { Custom = rule });
        return this;
    }

    /// <summary>
    /// Valida el objeto del campo (o cada elemento de la colección) con otro validador. Los campos de sus
    /// errores llevan delante la ruta de este (<c>address.street</c>, <c>items.0.name</c>).
    /// </summary>
    /// <remarks>
    /// <para>Un valor nulo no se valida (para eso está <see cref="Required"/>), salvo un elemento nulo de una
    /// colección, que da <see cref="ValidationErrorCodes.Required"/>. Un objeto que ya se está validando más arriba
    /// (un ciclo) no se vuelve a validar, y a más de 32 niveles se da <see cref="ValidationErrorCodes.Depth"/>.</para>
    /// </remarks>
    /// <typeparam name="TChild">Tipo del campo sin la marca de nulo (<c>Address</c> para un campo <c>Address?</c>).</typeparam>
    /// <param name="validator">Validador del tipo del campo.</param>
    /// <returns>Este mismo constructor de reglas.</returns>
    /// <exception cref="InvalidOperationException">Si el validador es de un tipo derivado del campo y no del mismo tipo.</exception>
    public RuleBuilder<T, TProperty> SetValidator<TChild>(Validator<TChild> validator)
        where TChild : TProperty
    {
        ArgumentNullException.ThrowIfNull(validator);

        if (typeof(TChild) != typeof(TProperty))
        {
            throw new InvalidOperationException($"SetValidator en el campo '{_path.Path}' necesita un validador de {typeof(TProperty).Name}, no de {typeof(TChild).Name}.");
        }

        _child = (value, context, path) => context.Nested((TChild)value, validator, path);
        return this;
    }

    /// <summary>
    /// Sólo se aplican las reglas de este campo si se cumple la condición.
    /// </summary>
    /// <param name="condition">Condición sobre el objeto entero.</param>
    /// <returns>Este mismo constructor de reglas.</returns>
    public RuleBuilder<T, TProperty> When(Func<T, bool> condition)
    {
        _when = condition ?? throw new ArgumentNullException(nameof(condition));
        return this;
    }

    /// <summary>
    /// Cambia el nombre del campo en los errores. Es obligatorio en las reglas sobre el objeto entero
    /// (<c>RuleFor(x => x)</c>), para que Angular sepa dónde pintar el error.
    /// </summary>
    /// <param name="name">Nombre o ruta del campo.</param>
    /// <returns>Este mismo constructor de reglas.</returns>
    public RuleBuilder<T, TProperty> WithName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _name = name;
        return this;
    }

    /// <summary>
    /// Cambia el mensaje de la última regla añadida.
    /// </summary>
    /// <param name="message">Mensaje.</param>
    /// <returns>Este mismo constructor de reglas.</returns>
    public RuleBuilder<T, TProperty> WithMessage(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        Last(nameof(WithMessage)).Message = message;
        return this;
    }

    /// <summary>
    /// Cambia el código de error de la última regla añadida.
    /// </summary>
    /// <param name="code">Código propio, con la forma <c>Modulo.Motivo</c>; el prefijo <c>Validations.</c> es de la librería.</param>
    /// <returns>Este mismo constructor de reglas.</returns>
    public RuleBuilder<T, TProperty> WithErrorCode(string code)
    {
        var check = Last(nameof(WithErrorCode));
        check.Code = ErrorCodeRules.Custom(code, nameof(code));
        return this;
    }

    /// <summary>
    /// Evalúa todas las reglas del campo aunque falle alguna (por defecto se para en el primer error del campo).
    /// </summary>
    /// <returns>Este mismo constructor de reglas.</returns>
    public RuleBuilder<T, TProperty> ContinueOnError()
    {
        _continueOnError = true;
        return this;
    }

    /// <inheritdoc/>
    void IRule<T>.Validate(T instance, ValidationContext context, string prefix)
    {
        if (_when is not null && !_when(instance))
        {
            return;
        }

        var path = MemberPath.Combine(prefix, _name ?? _path.Path);

        if (path.Length == 0)
        {
            throw new InvalidOperationException(RootWithoutName());
        }

        var raw = _path.GetValue(instance!);

        if (!_each)
        {
            Apply(instance, raw, path, context, element: false);
            return;
        }

        if (raw is not IEnumerable items)
        {
            return;
        }

        var index = 0;

        foreach (var item in items)
        {
            Apply(instance, item, path + "." + index.ToString(System.Globalization.CultureInfo.InvariantCulture), context, element: true);
            index++;
        }
    }

    /// <inheritdoc/>
    string? IRule<T>.Problem(string prefix)
        => _name is null && _path.IsRoot && prefix.Length == 0 ? RootWithoutName() : null;

    private static bool IsEmpty(TProperty value) => value switch
    {
        null => true,
        string text => string.IsNullOrWhiteSpace(text),
        IEnumerable items => !items.GetEnumerator().MoveNext(),
        _ => false,
    };

    private void Apply(T instance, object? raw, string path, ValidationContext context, bool element)
    {
        var isNull = raw is null;
        var value = isNull ? default! : (TProperty)raw!;
        var before = context.Errors.Count;

        foreach (var check in _checks)
        {
            if (!_continueOnError && context.Errors.Count > before)
            {
                return;
            }

            if (isNull && check.SkipNull)
            {
                continue;
            }

            if (check.Custom is not null)
            {
                check.Custom(value, new CustomRuleContext(context, path));
            }
            else if (!check.IsValid!(value, instance))
            {
                context.Add(path, check.Code, check.Message ?? check.DefaultMessage(path));
            }
        }

        if (_child is null || (!_continueOnError && context.Errors.Count > before))
        {
            return;
        }

        if (isNull)
        {
            if (element)
            {
                context.Add(path, ValidationErrorCodes.Required, Messages.For(ValidationErrorCodes.Required, path));
            }

            return;
        }

        _child(value!, context, path);
    }

    private RuleBuilder<T, TProperty> Add(string code, bool skipNull, Func<TProperty, T, bool> isValid, Func<string, string> defaultMessage)
    {
        _checks.Add(new Check(code, skipNull, isValid, defaultMessage));
        return this;
    }

    private RuleBuilder<T, TProperty> Pattern(string rule, string code, string pattern)
    {
        RequireString(rule);
        return Add(code, skipNull: true, (v, _) => ((string)(object)v!).Length == 0 || Patterns.IsMatch((string)(object)v!, pattern), f => Messages.For(code, f));
    }

    private RuleBuilder<T, TProperty> Compare(string rule, string code, TProperty limit, Func<int, bool> valid)
    {
        RequireComparable(rule);

        if (limit is null)
        {
            throw new ArgumentNullException(nameof(limit), $"El límite de {rule} no puede ser nulo.");
        }

        var comparer = Comparer<TProperty>.Default;
        return Add(code, skipNull: true, (v, _) => valid(comparer.Compare(v, limit)), f => Messages.For(code, f, limit));
    }

    private void RequireString(string rule)
    {
        if (typeof(TProperty) != typeof(string))
        {
            throw new InvalidOperationException($"{rule} sólo se puede usar con textos, y el campo '{_path.Path}' es {typeof(TProperty).Name}.");
        }
    }

    private void RequireComparable(string rule)
    {
        var type = Nullable.GetUnderlyingType(typeof(TProperty)) ?? typeof(TProperty);

        if (!typeof(IComparable).IsAssignableFrom(type) && !type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IComparable<>)))
        {
            throw new InvalidOperationException($"{rule} sólo se puede usar con valores comparables, y el campo '{_path.Path}' es {typeof(TProperty).Name}.");
        }
    }

    private Check Last(string modifier)
        => _checks.Count == 0 || _checks[^1].Custom is not null
            ? throw new InvalidOperationException($"{modifier} cambia la última regla añadida, y en el campo '{_path.Path}' no hay ninguna a la que se pueda aplicar.")
            : _checks[^1];

    private string RootWithoutName()
        => $"Una regla sobre el objeto entero de {typeof(T).Name} necesita WithName(\"campo\"), para que el error tenga un campo.";

    private sealed class Check
    {
        internal Check(string code, bool skipNull, Func<TProperty, T, bool>? isValid, Func<string, string> defaultMessage)
        {
            Code = code;
            SkipNull = skipNull;
            IsValid = isValid;
            DefaultMessage = defaultMessage;
        }

        internal string Code { get; set; }

        internal bool SkipNull { get; }

        internal Func<TProperty, T, bool>? IsValid { get; }

        internal Func<string, string> DefaultMessage { get; }

        internal string? Message { get; set; }

        internal Action<TProperty, CustomRuleContext>? Custom { get; init; }
    }
}
