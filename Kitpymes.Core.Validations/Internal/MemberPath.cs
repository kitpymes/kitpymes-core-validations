// -----------------------------------------------------------------------
// <copyright file="MemberPath.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations.Internal;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Cadena de propiedades o campos de una expresión (<c>x => x.Address.Street</c>): su ruta con el formato de
/// Angular (<c>address.street</c>) y la lectura del valor, que da nulo si un intermedio es nulo.
/// </summary>
internal sealed class MemberPath
{
    private readonly MemberInfo[] _members;

    private MemberPath(MemberInfo[] members, string path)
    {
        _members = members;
        Path = path;
    }

    internal string Path { get; }

    internal bool IsRoot => _members.Length == 0;

    internal static MemberPath From(LambdaExpression expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        var members = new List<MemberInfo>();
        var body = Unwrap(expression.Body);

        while (body is MemberExpression member && (member.Member is PropertyInfo || member.Member is FieldInfo))
        {
            members.Insert(0, member.Member);
            body = member.Expression is null ? null : Unwrap(member.Expression);
        }

        if (body is not ParameterExpression)
        {
            throw new ArgumentException(
                $"La regla '{expression}' sólo puede usar propiedades o campos del objeto (x => x.Propiedad.Otra).",
                nameof(expression));
        }

        return new MemberPath(members.ToArray(), string.Join(".", members.Select(JsonName)));
    }

    internal static string Combine(string prefix, string path)
        => prefix.Length == 0 ? path : path.Length == 0 ? prefix : prefix + "." + path;

    internal static string JsonName(MemberInfo member)
        => member.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name
            ?? JsonNamingPolicy.CamelCase.ConvertName(member.Name);

    internal object? GetValue(object root)
    {
        object? current = root;

        foreach (var member in _members)
        {
            if (current is null)
            {
                return null;
            }

            current = member is PropertyInfo property ? property.GetValue(current) : ((FieldInfo)member).GetValue(current);
        }

        return current;
    }

    private static Expression Unwrap(Expression expression)
        => expression is UnaryExpression unary
            && (unary.NodeType == ExpressionType.Convert || unary.NodeType == ExpressionType.ConvertChecked)
                ? Unwrap(unary.Operand)
                : expression;
}
