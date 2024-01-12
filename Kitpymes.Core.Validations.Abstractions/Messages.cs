// -----------------------------------------------------------------------
// <copyright file="Messages.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations;

using Kitpymes.Core.Shared;
using Kitpymes.Core.Shared.Extensions;

/*
    Configuración de los mensajes de error Messages
    Contiene las opciones de los mensajes de error
*/

/// <summary>
/// Configuración de los mensajes de error <c>Messages</c>.
/// Contiene las opciones de los mensajes de error.
/// </summary>
/// <remarks>
/// <para>En esta clase se pueden agregar todos los mensajes de error.</para>
/// </remarks>
public static class Messages
{
    /// <summary>
    /// Mensaje por defecto para formatos inválidos.
    /// </summary>
    /// <param name="fieldName">Nombre del campo.</param>
    /// <returns>Mensaje de error.</returns>
    public static string InvalidFormat(string? fieldName = null)
    => fieldName.ToIsNullOrEmpty()
        ? SharedResource.MsgInvalidFormat : SharedResource.MsgInvalidFormatWithFieldName.ToFormat(fieldName);

    /// <summary>
    /// Mensaje por defecto cuando el valor ingresado es nulo o vacío.
    /// </summary>
    /// <param name="fieldName">Nombre del campo.</param>
    /// <returns>Mensaje de error.</returns>
    public static string Required(string? fieldName = null)
    => fieldName.ToIsNullOrEmpty()
        ? SharedResource.MsgRequired : SharedResource.MsgRequiredWithFieldName.ToFormat(fieldName);

    /// <summary>
    /// Mensaje por defecto para formatos inválidos.
    /// </summary>
    /// <param name="fieldName">Nombre del campo.</param>
    /// <returns>Mensaje de error.</returns>
    public static string Regex(string? fieldName = null) => InvalidFormat(fieldName);

    /// <summary>
    /// Mensaje por defecto cuando una lista es nula o no contiene valores.
    /// </summary>
    /// <param name="fieldName">Nombre del campo.</param>
    /// <returns>Mensaje de error.</returns>
    public static string NullOrAny(string? fieldName = null) => Required(fieldName);

    /// <summary>
    /// Mensaje por defecto cuando el valor ingresado es nulo o vacío.
    /// </summary>
    /// <param name="fieldName">Nombre del campo.</param>
    /// <returns>Mensaje de error.</returns>
    public static string NullOrEmpty(string? fieldName = null) => Required(fieldName);

    /// <summary>
    /// Mensaje por defecto cuando dos campos comparados no son iguales.
    /// </summary>
    /// <param name="fieldName">Nombre del campo.</param>
    /// <param name="fieldNameCompare">Nombre del campo a comparar.</param>
    /// <returns>Mensaje de error.</returns>
    public static string Equal(string? fieldName = null, string? fieldNameCompare = null)
    {
        if (!fieldName.ToIsNullOrEmpty())
        {
            if (!fieldNameCompare.ToIsNullOrEmpty())
            {
                return SharedResource.MsgEqualWithFieldsName.ToFormat(fieldName, fieldNameCompare);
            }

            return SharedResource.MsgEqualWithFieldName.ToFormat(fieldName);
        }

        return SharedResource.MsgEqual;
    }

    /// <summary>
    /// Mensaje por defecto para cuando el valor ingresado es mayor que el configurado.
    /// </summary>
    /// <param name="max">Valor máximo que puede tener.</param>
    /// <param name="fieldName">Nombre del campo.</param>
    /// <returns>Mensaje de error.</returns>
    public static string Greater(long max, string? fieldName = null)
    => fieldName.ToIsNullOrEmpty()
        ? SharedResource.MsgGreater.ToFormat(max) : SharedResource.MsgGreaterWithFieldName.ToFormat(fieldName, max);

    /// <summary>
    /// Mensaje por defecto para cuando el valor ingresado es mayor o igual que el configurado.
    /// </summary>
    /// <param name="max">Valor máximo que puede tener.</param>
    /// <param name="fieldName">Nombre del campo.</param>
    /// <returns>Mensaje de error.</returns>
    public static string GreaterOrEqual(long max, string? fieldName = null)
    => fieldName.ToIsNullOrEmpty()
        ? SharedResource.MsgGreaterOrEqual.ToFormat(max) : SharedResource.MsgGreaterOrEqualWithFieldName.ToFormat(fieldName, max);

    /// <summary>
    /// Mensaje por defecto para cuando el valor ingresado es menor que el configurado.
    /// </summary>
    /// <param name="min">Valor mínimo que puede tener.</param>
    /// <param name="fieldName">Nombre del campo.</param>
    /// <returns>Mensaje de error.</returns>
    public static string Less(long min, string? fieldName = null)
    => fieldName.ToIsNullOrEmpty() ? SharedResource.MsgLess.ToFormat(min) : SharedResource.MsgLessWithFieldName.ToFormat(fieldName, min);

    /// <summary>
    /// Mensaje por defecto para cuando el valor ingresado es menor o igual que el configurado.
    /// </summary>
    /// <param name="min">Valor mínimo que puede tener.</param>
    /// <param name="fieldName">Nombre del campo.</param>
    /// <returns>Mensaje de error.</returns>
    public static string LessOrEqual(long min, string? fieldName = null)
    => fieldName.ToIsNullOrEmpty()
        ? SharedResource.MsgLessOrEqual.ToFormat(min) : SharedResource.MsgLessOrEqualWithFieldName.ToFormat(fieldName, min);

    /// <summary>
    /// Mensaje por defecto cuando el valor ingresado se encuentra contenido en los rangos configurados.
    /// </summary>
    /// <param name="min">Valor mínimo que puede tener.</param>
    /// <param name="max">Valor máximo que puede tener.</param>
    /// <param name="fieldName">Nombre del campo.</param>
    /// <returns>Mensaje de error.</returns>
    public static string InsideRange(long min, long max, string? fieldName = null)
    => fieldName.ToIsNullOrEmpty()
        ? SharedResource.MsgInsideRange.ToFormat(min, max) : SharedResource.MsgInsideRangeWithFieldName.ToFormat(fieldName, min, max);

    /// <summary>
    /// Mensaje por defecto cuando el valor ingresado se encuentra contenido o son iguales a los rangos configurados.
    /// </summary>
    /// <param name="min">Valor mínimo que puede tener.</param>
    /// <param name="max">Valor máximo que puede tener.</param>
    /// <param name="fieldName">Nombre del campo.</param>
    /// <returns>Mensaje de error.</returns>
    public static string InsideOrEqualRange(long min, long max, string? fieldName = null)
    => fieldName.ToIsNullOrEmpty()
        ? SharedResource.MsgInsideOrEqualRange.ToFormat(min, max) : SharedResource.MsgInsideOrEqualRangeWithFieldName.ToFormat(fieldName, min, max);

    /// <summary>
    /// Mensaje por defecto cuando el valor ingresado se encuentra fuera del rango configurado.
    /// </summary>
    /// <param name="min">Valor mínimo que puede tener.</param>
    /// <param name="max">Valor máximo que puede tener.</param>
    /// <param name="fieldName">Nombre del campo.</param>
    /// <returns>Mensaje de error.</returns>
    public static string OutRange(long min, long max, string? fieldName = null)
    => fieldName.ToIsNullOrEmpty()
        ? SharedResource.MsgOutRange.ToFormat(min, max) : SharedResource.MsgOutRangeWithFieldName.ToFormat(fieldName, min, max);

    /// <summary>
    /// Mensaje por defecto cuando el valor ingresado se encuentra fuera o es igual al rango configurado.
    /// </summary>
    /// <param name="min">Valor mínimo que puede tener.</param>
    /// <param name="max">Valor máximo que puede tener.</param>
    /// <param name="fieldName">Nombre del campo.</param>
    /// <returns>Mensaje de error.</returns>
    public static string OutOrEqualRange(long min, long max, string? fieldName = null)
    => fieldName.ToIsNullOrEmpty()
        ? SharedResource.MsgOutOrEqualRange.ToFormat(min, max) : SharedResource.MsgOutOrEqualRangeWithFieldName.ToFormat(fieldName, min, max);

    /// <summary>
    /// Mensaje por defecto cuando el path de un directorio no existe.
    /// </summary>
    /// <param name="value">Ruta del directorio.</param>
    /// <param name="fieldName">Nombre del campo.</param>
    /// <returns>Mensaje de error.</returns>
    public static string Directory(string? value = null, string? fieldName = null)
    {
        if (value.ToIsNullOrEmpty())
        {
            return fieldName.ToIsNullOrEmpty()
                ? SharedResource.MsgDirectory
                : SharedResource.MsgDirectoryWithFieldName.ToFormat(fieldName);
        }
        else
        {
            return fieldName.ToIsNullOrEmpty()
                ? SharedResource.MsgDirectoryWithValue.ToFormat(value)
                : SharedResource.MsgDirectoryWithValueAndFieldName.ToFormat(value, fieldName);
        }
    }

    /// <summary>
    /// Mensaje por defecto cuando el valor ingresado no tiene el formato correcto de un email.
    /// </summary>
    /// <param name="value">Ruta del directorio.</param>
    /// <param name="fieldName">Nombre del campo.</param>
    /// <returns>Mensaje de error.</returns>
    public static string Email(string? value = null, string? fieldName = null)
    {
        if (value.ToIsNullOrEmpty())
        {
            return fieldName.ToIsNullOrEmpty()
                ? SharedResource.MsgEmail
                : SharedResource.MsgEmailWithFieldName.ToFormat(fieldName);
        }
        else
        {
            return fieldName.ToIsNullOrEmpty()
                ? SharedResource.MsgEmailWithValue.ToFormat(value)
                : SharedResource.MsgEmailWithValueAndFieldName.ToFormat(value, fieldName);
        }
    }

    /// <summary>
    /// Mensaje por defecto cuando el path de un directorio no existe.
    /// </summary>
    /// <param name="value">Ruta del directorio.</param>
    /// <param name="fieldName">Nombre del campo.</param>
    /// <returns>Mensaje de error.</returns>
    public static string FileExtension(string? value = null, string? fieldName = null)
    {
        if (value.ToIsNullOrEmpty())
        {
            return fieldName.ToIsNullOrEmpty()
                ? SharedResource.MsgExtension
                : SharedResource.MsgExtensionWithFieldName.ToFormat(fieldName);
        }
        else
        {
            return fieldName.ToIsNullOrEmpty()
                ? SharedResource.MsgExtensionWithValue.ToFormat(value)
                : SharedResource.MsgExtensionWithValueAndFieldName.ToFormat(value, fieldName);
        }
    }

    /// <summary>
    /// Mensaje por defecto cuando el path de un directorio no existe.
    /// </summary>
    /// <param name="value">Ruta del directorio.</param>
    /// <param name="fieldName">Nombre del campo.</param>
    /// <returns>Mensaje de error.</returns>
    public static string File(string? value = null, string? fieldName = null)
    {
        if (value.ToIsNullOrEmpty())
        {
            return fieldName.ToIsNullOrEmpty()
                ? SharedResource.MsgFile
                : SharedResource.MsgFileWithFieldName.ToFormat(fieldName);
        }
        else
        {
            return fieldName.ToIsNullOrEmpty()
                ? SharedResource.MsgFileWithValue.ToFormat(value)
                : SharedResource.MsgFileWithValueAndFieldName.ToFormat(value, fieldName);
        }
    }

    /// <summary>
    /// Mensaje por defecto cuando el path de un directorio no existe.
    /// </summary>
    /// <param name="value">Ruta del directorio.</param>
    /// <param name="fieldName">Nombre del campo.</param>
    /// <returns>Mensaje de error.</returns>
    public static string Name(string? value = null, string? fieldName = null)
    {
        if (value.ToIsNullOrEmpty())
        {
            return fieldName.ToIsNullOrEmpty()
                ? SharedResource.MsgName
                : SharedResource.MsgNameWithFieldName.ToFormat(fieldName);
        }
        else
        {
            return fieldName.ToIsNullOrEmpty()
                ? SharedResource.MsgNameWithValue.ToFormat(value)
                : SharedResource.MsgNameWithValueAndFieldName.ToFormat(value, fieldName);
        }
    }

    /// <summary>
    /// Mensaje por defecto para cuando el valor ingresado es mayor que el configurado.
    /// </summary>
    /// <param name="min">Valor mínimo que puede tener.</param>
    /// <param name="fieldName">Nombre del campo.</param>
    /// <returns>Mensaje de error.</returns>
    public static string Password(long min, string? fieldName = null)
    => fieldName.ToIsNullOrEmpty()
            ? SharedResource.MsgPassword.ToFormat(min)
            : SharedResource.MsgPasswordWithFieldName.ToFormat(min, fieldName);

    /// <summary>
    /// Mensaje por defecto cuando el path de un directorio no existe.
    /// </summary>
    /// <param name="value">Ruta del directorio.</param>
    /// <param name="fieldName">Nombre del campo.</param>
    /// <returns>Mensaje de error.</returns>
    public static string Subdomain(string? value = null, string? fieldName = null)
    {
        if (value.ToIsNullOrEmpty())
        {
            return fieldName.ToIsNullOrEmpty()
                ? SharedResource.MsgSubdomain
                : SharedResource.MsgSubdomainWithFieldName.ToFormat(fieldName);
        }
        else
        {
            return fieldName.ToIsNullOrEmpty()
                ? SharedResource.MsgSubdomainWithValue.ToFormat(value)
                : SharedResource.MsgSubdomainWithValueAndFieldName.ToFormat(value, fieldName);
        }
    }
}
