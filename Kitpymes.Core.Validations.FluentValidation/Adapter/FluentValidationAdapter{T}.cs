// -----------------------------------------------------------------------
// <copyright file="FluentValidationAdapter{T}.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations.FluentValidation.Adapter;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Kitpymes.Core.Results;
using Kitpymes.Core.Validations.Abstractions;
using FV = global::FluentValidation;

/// <summary>
/// Adaptador de ejemplo: expone un validador de FluentValidation como <see cref="IValidator{T}"/>, para que el
/// paso de validación del mediador lo use igual que un <c>Validator&lt;T&gt;</c>.
/// </summary>
/// <remarks>
/// <para>Cada fallo de FluentValidation se convierte en un <see cref="FieldError"/> con la ruta de Angular
/// (<c>Items[0].Name</c> pasa a <c>items.0.name</c>) y un código <c>Validations.*</c> equivalente al de la regla
/// (<c>NotEmpty</c> da <c>Validations.Requerido</c>). Un código propio puesto con <c>WithErrorCode</c> se respeta si
/// tiene la forma <c>Modulo.Motivo</c> y no usa el prefijo <c>Validations.</c>; si no, queda
/// <c>Validations.Condicion</c>. Los fallos sin campo (reglas sobre el objeto entero sin
/// <c>OverridePropertyName</c>) van al mensaje del error.</para>
/// <para>Se registra con <c>LoadFluentValidationAdapters</c>.</para>
/// </remarks>
/// <typeparam name="T">Tipo que se valida.</typeparam>
public sealed class FluentValidationAdapter<T> : IValidator<T>
{
    /// <summary>
    /// Código del error general de validación (el mismo que <c>ValidationErrorCodes.Invalid</c>).
    /// </summary>
    private const string InvalidCode = "Validations.Invalido";

    /// <summary>
    /// Código de una regla sin equivalente (el mismo que <c>ValidationErrorCodes.Condition</c>).
    /// </summary>
    private const string ConditionCode = "Validations.Condicion";

    private const string Prefix = "Validations.";

    private static readonly Dictionary<string, string> Codes = new (StringComparer.Ordinal)
    {
        ["NotNullValidator"] = "Validations.Requerido",
        ["NotEmptyValidator"] = "Validations.Requerido",
        ["MaximumLengthValidator"] = "Validations.LongitudMaxima",
        ["MinimumLengthValidator"] = "Validations.LongitudMinima",
        ["EmailValidator"] = "Validations.Email",
        ["RegularExpressionValidator"] = "Validations.Formato",
        ["GreaterThanValidator"] = "Validations.Mayor",
        ["GreaterThanOrEqualValidator"] = "Validations.MayorOIgual",
        ["LessThanValidator"] = "Validations.Menor",
        ["LessThanOrEqualValidator"] = "Validations.MenorOIgual",
        ["InclusiveBetweenValidator"] = "Validations.Rango",
        ["EqualValidator"] = "Validations.Igual",
        ["NotEqualValidator"] = "Validations.Distinto",
        ["EnumValidator"] = "Validations.EnumValido",
    };

    private readonly FV.IValidator<T> _validator;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="FluentValidationAdapter{T}"/>.
    /// </summary>
    /// <param name="validators">Validadores de FluentValidation del tipo; tiene que haber exactamente uno.</param>
    /// <exception cref="InvalidOperationException">Si no hay ninguno o hay más de uno (al resolverlo, la comprobación de arranque lo informa).</exception>
    public FluentValidationAdapter(IEnumerable<FV.IValidator<T>> validators)
    {
        ArgumentNullException.ThrowIfNull(validators);

        var list = validators.ToList();

        if (list.Count != 1 || list[0] is null)
        {
            throw new InvalidOperationException(
                $"{typeof(T).Name} tiene {list.Count} validadores de FluentValidation y el adaptador necesita exactamente uno.");
        }

        _validator = list[0];
    }

    private static bool English => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "en";

    /// <inheritdoc/>
    public async ValueTask<Result> ValidateAsync(T instance, CancellationToken cancellationToken = default)
    {
        if (instance is null)
        {
            var nullMessage = English
                ? $"The object to validate ({typeof(T).Name}) is null."
                : $"El objeto a validar ({typeof(T).Name}) es nulo.";

            return Result.Failure(Error.Validation(InvalidCode, nullMessage));
        }

        var validation = await _validator.ValidateAsync(instance, cancellationToken).ConfigureAwait(false);

        if (validation.IsValid)
        {
            return Result.Success();
        }

        var fields = new List<FieldError>();
        var general = new List<string>();

        foreach (var failure in validation.Errors)
        {
            var field = Path(failure.PropertyName);
            var message = string.IsNullOrWhiteSpace(failure.ErrorMessage) ? DefaultMessage(field) : failure.ErrorMessage;

            if (field.Length == 0)
            {
                general.Add(message);
            }
            else
            {
                fields.Add(new FieldError(field, Code(failure.ErrorCode), message));
            }
        }

        var text = English ? "There are validation errors." : "Hay errores de validación.";

        return Result.Failure(Error.Validation(
            InvalidCode,
            general.Count == 0 ? text : text + " " + string.Join(" ", general),
            fields));
    }

    /// <summary>
    /// Convierte la ruta de FluentValidation en la de Angular: <c>Items[0].Name</c> en <c>items.0.name</c>.
    /// Lo que va entre corchetes (índices, claves) se deja tal cual.
    /// </summary>
    /// <param name="propertyName">Ruta de FluentValidation.</param>
    /// <returns>La ruta, o vacío si el fallo no tiene campo.</returns>
    private static string Path(string? propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            return string.Empty;
        }

        var parts = new List<string>();
        var current = new StringBuilder();
        var i = 0;

        while (i < propertyName.Length)
        {
            var c = propertyName[i];

            if (c == '.')
            {
                Flush(parts, current);
                i++;
            }
            else if (c == '[')
            {
                Flush(parts, current);
                var end = propertyName.IndexOf(']', i + 1);
                end = end < 0 ? propertyName.Length : end;
                var key = propertyName.Substring(i + 1, end - i - 1);

                if (key.Length > 0)
                {
                    parts.Add(key);
                }

                i = end + 1;
            }
            else
            {
                current.Append(c);
                i++;
            }
        }

        Flush(parts, current);

        return string.Join(".", parts);
    }

    /// <summary>
    /// Código <c>Validations.*</c> de un fallo de FluentValidation.
    /// </summary>
    /// <param name="errorCode">Código de FluentValidation (el nombre de la regla, o el puesto con <c>WithErrorCode</c>).</param>
    /// <returns>El código para el <see cref="FieldError"/>.</returns>
    private static string Code(string? errorCode)
    {
        if (errorCode is null)
        {
            return ConditionCode;
        }

        if (Codes.TryGetValue(errorCode, out var code))
        {
            return code;
        }

        return IsValidCode(errorCode) && !errorCode.StartsWith(Prefix, StringComparison.Ordinal) ? errorCode : ConditionCode;
    }

    private static void Flush(List<string> parts, StringBuilder current)
    {
        var part = current.ToString().Trim();
        current.Clear();

        if (part.Length > 0)
        {
            parts.Add(JsonNamingPolicy.CamelCase.ConvertName(part));
        }
    }

    private static string DefaultMessage(string field)
        => English
            ? (field.Length == 0 ? "The object is not valid." : $"The field '{field}' is not valid.")
            : (field.Length == 0 ? "El objeto no es válido." : $"El campo '{field}' no es válido.");

    private static bool IsValidCode(string code)
        => code.Split('.').Length >= 2 && code.Split('.').All(part => part.Length > 0
            && part[0] >= 'A' && part[0] <= 'Z'
            && part.All(c => (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9')));
}
