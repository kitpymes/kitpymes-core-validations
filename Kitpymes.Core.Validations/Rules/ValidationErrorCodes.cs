// -----------------------------------------------------------------------
// <copyright file="ValidationErrorCodes.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations;

/// <summary>
/// Códigos de error de las reglas de la librería, para que Angular traduzca los mensajes.
/// </summary>
/// <remarks>
/// <para>Un código publicado no se renombra: es contrato con el frontend. El prefijo <c>Validations.</c> es de la
/// librería y no se puede usar en códigos propios.</para>
/// </remarks>
public static class ValidationErrorCodes
{
    /// <summary>
    /// Código del error de validación que agrupa los errores por campo.
    /// </summary>
    public const string Invalid = "Validations.Invalido";

    /// <summary>
    /// Campo obligatorio: nulo, texto vacío o en blanco, o colección vacía.
    /// </summary>
    public const string Required = "Validations.Requerido";

    /// <summary>
    /// Texto más largo que el máximo.
    /// </summary>
    public const string MaxLength = "Validations.LongitudMaxima";

    /// <summary>
    /// Texto más corto que el mínimo.
    /// </summary>
    public const string MinLength = "Validations.LongitudMinima";

    /// <summary>
    /// Email con formato no válido.
    /// </summary>
    public const string Email = "Validations.Email";

    /// <summary>
    /// Nombre con caracteres no válidos.
    /// </summary>
    public const string Name = "Validations.Nombre";

    /// <summary>
    /// Subdominio con caracteres no válidos.
    /// </summary>
    public const string Subdomain = "Validations.Subdominio";

    /// <summary>
    /// Texto que no cumple la expresión regular.
    /// </summary>
    public const string Format = "Validations.Formato";

    /// <summary>
    /// Valor que no es mayor que el límite.
    /// </summary>
    public const string GreaterThan = "Validations.Mayor";

    /// <summary>
    /// Valor que no es mayor o igual que el límite.
    /// </summary>
    public const string GreaterThanOrEqual = "Validations.MayorOIgual";

    /// <summary>
    /// Valor que no es menor que el límite.
    /// </summary>
    public const string LessThan = "Validations.Menor";

    /// <summary>
    /// Valor que no es menor o igual que el límite.
    /// </summary>
    public const string LessThanOrEqual = "Validations.MenorOIgual";

    /// <summary>
    /// Valor fuera del rango (límites incluidos).
    /// </summary>
    public const string Between = "Validations.Rango";

    /// <summary>
    /// Valor distinto del esperado.
    /// </summary>
    public const string Equal = "Validations.Igual";

    /// <summary>
    /// Valor igual a uno que no se admite.
    /// </summary>
    public const string NotEqual = "Validations.Distinto";

    /// <summary>
    /// Valor que no está definido en el enumerado.
    /// </summary>
    public const string Enum = "Validations.EnumValido";

    /// <summary>
    /// Condición propia (<c>Must</c>) que no se cumple.
    /// </summary>
    public const string Condition = "Validations.Condicion";

    /// <summary>
    /// Objeto anidado más profundo que el límite.
    /// </summary>
    public const string Depth = "Validations.Profundidad";

    /// <summary>
    /// Código del error de configuración de la validación al arrancar.
    /// </summary>
    public const string Configuration = "Validations.Configuracion";
}
