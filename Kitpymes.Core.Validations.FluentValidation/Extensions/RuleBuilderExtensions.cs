// -----------------------------------------------------------------------
// <copyright file="RuleBuilderExtensions.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations.FluentValidation
{
    using System.Collections;
    using global::FluentValidation;
    using Kitpymes.Core.Shared.Extensions;

    /*
        Clase de extensión RuleBuilderExtensions
        Contiene las extensiones de las validaciones custom
    */

    /// <summary>
    /// Clase de extensión <c>RuleBuilderExtensions</c>.
    /// Contiene las extensiones de las validaciones custom.
    /// </summary>
    /// <remarks>
    /// <para>En esta clase se pueden agregar todas las validaciones custom.</para>
    /// </remarks>
    public static class RuleBuilderExtensions
    {
        /// <summary>
        /// Comprueba si el valor ingresado es valido.
        /// </summary>
        /// <typeparam name="T">Entidad a validar.</typeparam>
        /// <typeparam name="TProperty">Propiedad de la entidad.</typeparam>
        /// <param name="ruleBuilder">Regla de validación.</param>
        /// <param name="overrideFieldName">Nombre del campo.</param>
        /// <returns>IRuleBuilder.</returns>
        public static IRuleBuilder<T, TProperty> IsNullOrAny<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, string? overrideFieldName = null)
            where TProperty : IEnumerable?
        => ruleBuilder.Custom((value, context) =>
        {
            if (value is not IEnumerable || value.ToIsNullOrAny())
            {
                context.AddFailure(Messages.NullOrAny(overrideFieldName ?? context.DisplayName));
            }
        });

        /// <summary>
        /// Comprueba si el valor ingresado es valido.
        /// </summary>
        /// <typeparam name="T">Entidad a validar.</typeparam>
        /// <typeparam name="TProperty">Propiedad de la entidad.</typeparam>
        /// <param name="ruleBuilder">Regla de validación.</param>
        /// <param name="message">Mensaje a mostrar.</param>
        /// <returns>IRuleBuilder.</returns>
        public static IRuleBuilder<T, TProperty> IsNullOrAnyWithMessage<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, string message)
           where TProperty : IEnumerable?
           => ruleBuilder.Custom((value, context) =>
           {
               if (value is not IEnumerable || value.ToIsNullOrAny())
               {
                   context.AddFailure(message);
               }
           });

        /// <summary>
        /// Comprueba si el valor ingresado es valido.
        /// </summary>
        /// <typeparam name="T">Entidad a validar.</typeparam>
        /// <typeparam name="TProperty">Propiedad de la entidad.</typeparam>
        /// <param name="ruleBuilder">Regla de validación.</param>
        /// <param name="valueCompare">Valor a comparar.</param>
        /// <param name="fieldName">Nombre del campo.</param>
        /// <param name="fieldNameCompare">Nombre del campo a comparar.</param>
        /// <returns>IRuleBuilder.</returns>
        public static IRuleBuilder<T, TProperty> IsNotEqual<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, TProperty valueCompare, string? fieldName = null, string? fieldNameCompare = null)
         => ruleBuilder.Custom((value, context) =>
         {
             if (!value.ToIsNullOrEmpty() && value.ToIsNotEqual(valueCompare))
             {
                 context.AddFailure(Messages.Equal(fieldName ?? context.DisplayName, fieldNameCompare));
             }
         });

        /// <summary>
        /// Comprueba si el valor ingresado es valido.
        /// </summary>
        /// <typeparam name="T">Entidad a validar.</typeparam>
        /// <typeparam name="TProperty">Propiedad de la entidad.</typeparam>
        /// <param name="ruleBuilder">Regla de validación.</param>
        /// <param name="valueCompare">Valor a comparar.</param>
        /// <param name="message">Mensaje a mostrar.</param>
        /// <returns>IRuleBuilder.</returns>
        public static IRuleBuilder<T, TProperty> IsNotEqualWithMessage<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, TProperty valueCompare, string message)
         => ruleBuilder.Custom((value, context) =>
         {
             if (!value.ToIsNullOrEmpty() && value.ToIsNotEqual(valueCompare))
             {
                 context.AddFailure(message);
             }
         });

        /// <summary>
        /// Comprueba si el valor ingresado es valido.
        /// </summary>
        /// <typeparam name="T">Entidad a validar.</typeparam>
        /// <typeparam name="TProperty">Propiedad de la entidad.</typeparam>
        /// <param name="ruleBuilder">Regla de validación.</param>
        /// <param name="max">Valor máximo que puede tener el campo.</param>
        /// <param name="fieldName">Nombre del campo.</param>
        /// <returns>IRuleBuilder.</returns>
        public static IRuleBuilder<T, TProperty> IsGreater<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, long max, string? fieldName = null)
        => ruleBuilder.Custom((value, context) =>
        {
            if (!value.ToIsNullOrEmpty() && value.ToIsGreater(max))
            {
                context.AddFailure(Messages.Greater(max, fieldName ?? context.DisplayName));
            }
        });

        /// <summary>
        /// Comprueba si el valor ingresado es valido.
        /// </summary>
        /// <typeparam name="T">Entidad a validar.</typeparam>
        /// <typeparam name="TProperty">Propiedad de la entidad.</typeparam>
        /// <param name="ruleBuilder">Regla de validación.</param>
        /// <param name="max">Valor máximo que puede tener el campo.</param>
        /// <param name="message">Mensaje a mostrar.</param>
        /// <returns>IRuleBuilder.</returns>
        public static IRuleBuilder<T, TProperty> IsGreaterWithMessage<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, long max, string message)
        => ruleBuilder.Custom((value, context) =>
        {
            if (!value.ToIsNullOrEmpty() && value.ToIsGreater(max))
            {
                context.AddFailure(message);
            }
        });

        /// <summary>
        /// Comprueba si el valor ingresado es valido.
        /// </summary>
        /// <typeparam name="T">Entidad a validar.</typeparam>
        /// <typeparam name="TProperty">Propiedad de la entidad.</typeparam>
        /// <param name="ruleBuilder">Regla de validación.</param>
        /// <param name="min">Valor mínimo que puede tener el campo.</param>
        /// <param name="fieldName">Nombre del campo.</param>
        /// <returns>IRuleBuilder.</returns>
        public static IRuleBuilder<T, TProperty> IsLess<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, long min, string? fieldName = null)
        => ruleBuilder.Custom((value, context) =>
        {
            if (!value.ToIsNullOrEmpty() && value.ToIsLess(min))
            {
                context.AddFailure(Messages.Less(min, fieldName ?? context.DisplayName));
            }
        });

        /// <summary>
        /// Comprueba si el valor ingresado es valido.
        /// </summary>
        /// <typeparam name="T">Entidad a validar.</typeparam>
        /// <typeparam name="TProperty">Propiedad de la entidad.</typeparam>
        /// <param name="ruleBuilder">Regla de validación.</param>
        /// <param name="min">Valor mínimo que puede tener el campo.</param>
        /// <param name="message">Mensaje a mostrar.</param>
        /// <returns>IRuleBuilder.</returns>
        public static IRuleBuilder<T, TProperty> IsLessWithMessage<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, long min, string message)
        => ruleBuilder.Custom((value, context) =>
        {
            if (!value.ToIsNullOrEmpty() && value.ToIsLess(min))
            {
                context.AddFailure(message);
            }
        });

        /// <summary>
        /// Comprueba si el valor ingresado es valido.
        /// </summary>
        /// <typeparam name="T">Entidad a validar.</typeparam>
        /// <typeparam name="TProperty">Propiedad de la entidad.</typeparam>
        /// <param name="ruleBuilder">Regla de validación.</param>
        /// <param name="fieldName">Nombre del campo.</param>
        /// <returns>IRuleBuilder.</returns>
        public static IRuleBuilder<T, TProperty> IsNullOrEmpty<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, string? fieldName = null)
        => ruleBuilder.Custom((value, context) =>
        {
            if (value.ToIsNullOrEmpty())
            {
                context.AddFailure(Messages.NullOrEmpty(fieldName ?? context.DisplayName));
            }
        });

        /// <summary>
        /// Comprueba si el valor ingresado es valido.
        /// </summary>
        /// <typeparam name="T">Entidad a validar.</typeparam>
        /// <typeparam name="TProperty">Propiedad de la entidad.</typeparam>
        /// <param name="ruleBuilder">Regla de validación.</param>
        /// <param name="message">Mensaje a mostrar.</param>
        /// <returns>IRuleBuilder.</returns>
        public static IRuleBuilder<T, TProperty> IsNullOrEmptyWithMessage<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, string message)
        => ruleBuilder.Custom((value, context) =>
        {
            if (value.ToIsNullOrEmpty())
            {
                context.AddFailure(message);
            }
        });

        /// <summary>
        /// Comprueba si el valor ingresado es valido.
        /// </summary>
        /// <typeparam name="T">Entidad a validar.</typeparam>
        /// <typeparam name="TProperty">Propiedad de la entidad.</typeparam>
        /// <param name="ruleBuilder">Regla de validación.</param>
        /// <param name="min">Valor mínimo que puede tener el campo.</param>
        /// <param name="max">Valor máximo que puede tener el campo.</param>
        /// <param name="fieldName">Nombre del campo.</param>
        /// <returns>IRuleBuilder.</returns>
        public static IRuleBuilder<T, TProperty> IsInsideRange<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, long min, long max, string? fieldName = null)
         => ruleBuilder.Custom((value, context) =>
         {
             if (!value.ToIsNullOrEmpty() && value.ToIsInsideRange(min, max))
             {
                 context.AddFailure(Messages.InsideRange(min, max, fieldName ?? context.DisplayName));
             }
         });

        /// <summary>
        /// Comprueba si el valor ingresado es valido.
        /// </summary>
        /// <typeparam name="T">Entidad a validar.</typeparam>
        /// <typeparam name="TProperty">Propiedad de la entidad.</typeparam>
        /// <param name="ruleBuilder">Regla de validación.</param>
        /// <param name="min">Valor mínimo que puede tener el campo.</param>
        /// <param name="max">Valor máximo que puede tener el campo.</param>
        /// <param name="message">Mensaje a mostrar.</param>
        /// <returns>IRuleBuilder.</returns>
        public static IRuleBuilder<T, TProperty> IsInsideRangeWithMessage<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, long min, long max, string message)
         => ruleBuilder.Custom((value, context) =>
         {
             if (!value.ToIsNullOrEmpty() && value.ToIsInsideRange(min, max))
             {
                 context.AddFailure(message);
             }
         });

        /// <summary>
        /// Comprueba si el valor ingresado es valido.
        /// </summary>
        /// <typeparam name="T">Entidad a validar.</typeparam>
        /// <typeparam name="TProperty">Propiedad de la entidad.</typeparam>
        /// <param name="ruleBuilder">Regla de validación.</param>
        /// <param name="min">Valor mínimo que puede tener el campo.</param>
        /// <param name="max">Valor máximo que puede tener el campo.</param>
        /// <param name="fieldName">Nombre del campo.</param>
        /// <returns>IRuleBuilder.</returns>
        public static IRuleBuilder<T, TProperty> IsInsideOrEqualRange<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, long min, long max, string? fieldName = null)
         => ruleBuilder.Custom((value, context) =>
         {
             if (!value.ToIsNullOrEmpty() && value.ToIsInsideOrEqualRange(min, max))
             {
                 context.AddFailure(Messages.InsideOrEqualRange(min, max, fieldName ?? context.DisplayName));
             }
         });

        /// <summary>
        /// Comprueba si el valor ingresado es valido.
        /// </summary>
        /// <typeparam name="T">Entidad a validar.</typeparam>
        /// <typeparam name="TProperty">Propiedad de la entidad.</typeparam>
        /// <param name="ruleBuilder">Regla de validación.</param>
        /// <param name="min">Valor mínimo que puede tener el campo.</param>
        /// <param name="max">Valor máximo que puede tener el campo.</param>
        /// <param name="message">Mensaje a mostrar.</param>
        /// <returns>IRuleBuilder.</returns>
        public static IRuleBuilder<T, TProperty> IsInsideOrEqualRangeWithMessage<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, long min, long max, string message)
         => ruleBuilder.Custom((value, context) =>
         {
             if (!value.ToIsNullOrEmpty() && value.ToIsInsideOrEqualRange(min, max))
             {
                 context.AddFailure(message);
             }
         });

        /// <summary>
        /// Comprueba si el valor ingresado es valido.
        /// </summary>
        /// <typeparam name="T">Entidad a validar.</typeparam>
        /// <typeparam name="TProperty">Propiedad de la entidad.</typeparam>
        /// <param name="ruleBuilder">Regla de validación.</param>
        /// <param name="min">Valor mínimo que puede tener el campo.</param>
        /// <param name="max">Valor máximo que puede tener el campo.</param>
        /// <param name="fieldName">Nombre del campo.</param>
        /// <returns>IRuleBuilder.</returns>
        public static IRuleBuilder<T, TProperty> IsOutRange<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, long min, long max, string? fieldName = null)
         => ruleBuilder.Custom((value, context) =>
         {
             if (!value.ToIsNullOrEmpty() && value.ToIsOutRange(min, max))
             {
                 context.AddFailure(Messages.OutRange(min, max, fieldName ?? context.DisplayName));
             }
         });

        /// <summary>
        /// Comprueba si el valor ingresado es valido.
        /// </summary>
        /// <typeparam name="T">Entidad a validar.</typeparam>
        /// <typeparam name="TProperty">Propiedad de la entidad.</typeparam>
        /// <param name="ruleBuilder">Regla de validación.</param>
        /// <param name="min">Valor mínimo que puede tener el campo.</param>
        /// <param name="max">Valor máximo que puede tener el campo.</param>
        /// <param name="message">Mensaje a mostrar.</param>
        /// <returns>IRuleBuilder.</returns>
        public static IRuleBuilder<T, TProperty> IsOutRangeWithMessage<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, long min, long max, string message)
         => ruleBuilder.Custom((value, context) =>
         {
             if (!value.ToIsNullOrEmpty() && value.ToIsOutRange(min, max))
             {
                 context.AddFailure(message);
             }
         });

        /// <summary>
        /// Comprueba si el valor ingresado es valido.
        /// </summary>
        /// <typeparam name="T">Entidad a validar.</typeparam>
        /// <typeparam name="TProperty">Propiedad de la entidad.</typeparam>
        /// <param name="ruleBuilder">Regla de validación.</param>
        /// <param name="min">Valor mínimo que puede tener el campo.</param>
        /// <param name="max">Valor máximo que puede tener el campo.</param>
        /// <param name="fieldName">Nombre del campo.</param>
        /// <returns>IRuleBuilder.</returns>
        public static IRuleBuilder<T, TProperty> IsOutOrEqualRange<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, long min, long max, string? fieldName = null)
         => ruleBuilder.Custom((value, context) =>
         {
             if (!value.ToIsNullOrEmpty() && value.ToIsOutOrEqualRange(min, max))
             {
                 context.AddFailure(Messages.OutOrEqualRange(min, max, fieldName ?? context.DisplayName));
             }
         });

        /// <summary>
        /// Comprueba si el valor ingresado es valido.
        /// </summary>
        /// <typeparam name="T">Entidad a validar.</typeparam>
        /// <typeparam name="TProperty">Propiedad de la entidad.</typeparam>
        /// <param name="ruleBuilder">Regla de validación.</param>
        /// <param name="min">Valor mínimo que puede tener el campo.</param>
        /// <param name="max">Valor máximo que puede tener el campo.</param>
        /// <param name="message">Mensaje a mostrar.</param>
        /// <returns>IRuleBuilder.</returns>
        public static IRuleBuilder<T, TProperty> IsOutOrEqualRangeWithMessage<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, long min, long max, string message)
         => ruleBuilder.Custom((value, context) =>
         {
             if (!value.ToIsNullOrEmpty() && value.ToIsOutOrEqualRange(min, max))
             {
                 context.AddFailure(message);
             }
         });

        /// <summary>
        /// Comprueba si el valor ingresado es valido.
        /// </summary>
        /// <typeparam name="T">Entidad a validar.</typeparam>
        /// <param name="ruleBuilder">Regla de validación.</param>
        /// <param name="regex">Expresión regular que tiene que cumplir el campo.</param>
        /// <param name="fieldName">Nombre del campo.</param>
        /// <returns>IRuleBuilder.</returns>
        public static IRuleBuilder<T, string?> IsRegex<T>(this IRuleBuilder<T, string?> ruleBuilder, string regex, string? fieldName = null)
        => ruleBuilder.Custom((value, context) =>
        {
            if (!value.ToIsNullOrEmpty() && !value.ToIsRegexMatch(regex))
            {
                context.AddFailure(Messages.Regex(fieldName ?? context.DisplayName));
            }
        });

        /// <summary>
        /// Comprueba si el valor ingresado es valido.
        /// </summary>
        /// <typeparam name="T">Entidad a validar.</typeparam>
        /// <param name="ruleBuilder">Regla de validación.</param>
        /// <param name="regex">Expresión regular que tiene que cumplir el campo.</param>
        /// <param name="message">Mensaje a mostrar.</param>
        /// <returns>IRuleBuilder.</returns>
        public static IRuleBuilder<T, string?> IsRegexWithMessage<T>(this IRuleBuilder<T, string?> ruleBuilder, string regex, string message)
        => ruleBuilder.Custom((value, context) =>
        {
            if (!value.ToIsNullOrEmpty() && value.ToIsRegexMatch(regex))
            {
                context.AddFailure(message);
            }
        });

        /// <summary>
        /// Comprueba si el valor ingresado es valido.
        /// </summary>
        /// <typeparam name="T">Entidad a validar.</typeparam>
        /// <param name="ruleBuilder">Regla de validación.</param>
        /// <param name="fieldName">Nombre del campo.</param>
        /// <returns>IRuleBuilder.</returns>
        public static IRuleBuilder<T, string?> IsDirectory<T>(this IRuleBuilder<T, string?> ruleBuilder, string? fieldName = null)
        => ruleBuilder.Custom((value, context) =>
        {
            if (!value.ToIsNullOrEmpty() && !value.ToIsDirectory())
            {
                context.AddFailure(Messages.Directory(value, fieldName ?? context.DisplayName));
            }
        });

        /// <summary>
        /// Comprueba si el valor ingresado es valido.
        /// </summary>
        /// <typeparam name="T">Entidad a validar.</typeparam>
        /// <param name="ruleBuilder">Regla de validación.</param>
        /// <param name="message">Mensaje a mostrar.</param>
        /// <returns>IRuleBuilder.</returns>
        public static IRuleBuilder<T, string?> IsDirectoryWithMessage<T>(this IRuleBuilder<T, string?> ruleBuilder, string message)
        => ruleBuilder.Custom((value, context) =>
        {
            if (!value.ToIsNullOrEmpty() && !value.ToIsDirectory())
            {
                context.AddFailure(message);
            }
        });

        /// <summary>
        /// Comprueba si el valor ingresado es valido.
        /// </summary>
        /// <typeparam name="T">Entidad a validar.</typeparam>
        /// <param name="ruleBuilder">Regla de validación.</param>
        /// <param name="fieldName">Nombre del campo.</param>
        /// <returns>IRuleBuilder.</returns>
        public static IRuleBuilder<T, string?> IsEmail<T>(this IRuleBuilder<T, string?> ruleBuilder, string? fieldName = null)
        => ruleBuilder.Custom((value, context) =>
        {
            if (!value.ToIsNullOrEmpty() && !value.ToIsEmail())
            {
                context.AddFailure(Messages.Email(value, fieldName ?? context.DisplayName));
            }
        });

        /// <summary>
        /// Comprueba si el valor ingresado es valido.
        /// </summary>
        /// <typeparam name="T">Entidad a validar.</typeparam>
        /// <param name="ruleBuilder">Regla de validación.</param>
        /// <param name="message">Mensaje a mostrar.</param>
        /// <returns>IRuleBuilder.</returns>
        public static IRuleBuilder<T, string?> IsEmailWithMessage<T>(this IRuleBuilder<T, string?> ruleBuilder, string message)
        => ruleBuilder.Custom((value, context) =>
        {
            if (!value.ToIsNullOrEmpty() && !value.ToIsEmail())
            {
                context.AddFailure(message);
            }
        });

        /// <summary>
        /// Comprueba si el valor ingresado es valido.
        /// </summary>
        /// <typeparam name="T">Entidad a validar.</typeparam>
        /// <param name="ruleBuilder">Regla de validación.</param>
        /// <param name="fieldName">Nombre del campo.</param>
        /// <returns>IRuleBuilder.</returns>
        public static IRuleBuilder<T, string?> IsFileExtension<T>(this IRuleBuilder<T, string?> ruleBuilder, string? fieldName = null)
        => ruleBuilder.Custom((value, context) =>
        {
            if (!value.ToIsNullOrEmpty() && !value.ToIsFileExtension())
            {
                context.AddFailure(Messages.FileExtension(value, fieldName ?? context.DisplayName));
            }
        });

        /// <summary>
        /// Comprueba si el valor ingresado es valido.
        /// </summary>
        /// <typeparam name="T">Entidad a validar.</typeparam>
        /// <param name="ruleBuilder">Regla de validación.</param>
        /// <param name="message">Mensaje a mostrar.</param>
        /// <returns>IRuleBuilder.</returns>
        public static IRuleBuilder<T, string?> IsFileExtensionWithMessage<T>(this IRuleBuilder<T, string?> ruleBuilder, string message)
        => ruleBuilder.Custom((value, context) =>
        {
            if (!value.ToIsNullOrEmpty() && !value.ToIsFileExtension())
            {
                context.AddFailure(message);
            }
        });

        /// <summary>
        /// Comprueba si el valor ingresado es valido.
        /// </summary>
        /// <typeparam name="T">Entidad a validar.</typeparam>
        /// <param name="ruleBuilder">Regla de validación.</param>
        /// <param name="fieldName">Nombre del campo.</param>
        /// <returns>IRuleBuilder.</returns>
        public static IRuleBuilder<T, string?> IsFile<T>(this IRuleBuilder<T, string?> ruleBuilder, string? fieldName = null)
        => ruleBuilder.Custom((value, context) =>
        {
            if (!value.ToIsNullOrEmpty() && !value.ToIsFile())
            {
                context.AddFailure(Messages.File(value, fieldName ?? context.DisplayName));
            }
        });

        /// <summary>
        /// Comprueba si el valor ingresado es valido.
        /// </summary>
        /// <typeparam name="T">Entidad a validar.</typeparam>
        /// <param name="ruleBuilder">Regla de validación.</param>
        /// <param name="message">Mensaje a mostrar.</param>
        /// <returns>IRuleBuilder.</returns>
        public static IRuleBuilder<T, string?> IsFileWithMessage<T>(this IRuleBuilder<T, string?> ruleBuilder, string message)
        => ruleBuilder.Custom((value, context) =>
        {
            if (!value.ToIsNullOrEmpty() && !value.ToIsFile())
            {
                context.AddFailure(message);
            }
        });

        /// <summary>
        /// Comprueba si el valor ingresado es valido.
        /// </summary>
        /// <typeparam name="T">Entidad a validar.</typeparam>
        /// <param name="ruleBuilder">Regla de validación.</param>
        /// <param name="fieldName">Nombre del campo.</param>
        /// <returns>IRuleBuilder.</returns>
        public static IRuleBuilder<T, string?> IsName<T>(this IRuleBuilder<T, string?> ruleBuilder, string? fieldName = null)
        => ruleBuilder.Custom((value, context) =>
        {
            if (!value.ToIsNullOrEmpty() && !value.ToIsName())
            {
                context.AddFailure(Messages.Name(value, fieldName ?? context.DisplayName));
            }
        });

        /// <summary>
        /// Comprueba si el valor ingresado es valido.
        /// </summary>
        /// <typeparam name="T">Entidad a validar.</typeparam>
        /// <param name="ruleBuilder">Regla de validación.</param>
        /// <param name="message">Mensaje a mostrar.</param>
        /// <returns>IRuleBuilder.</returns>
        public static IRuleBuilder<T, string?> IsNameWithMessage<T>(this IRuleBuilder<T, string?> ruleBuilder, string message)
        => ruleBuilder.Custom((value, context) =>
        {
            if (!value.ToIsNullOrEmpty() && !value.ToIsName())
            {
                context.AddFailure(message);
            }
        });

        /// <summary>
        /// Comprueba si el valor ingresado es valido.
        /// </summary>
        /// <typeparam name="T">Entidad a validar.</typeparam>
        /// <param name="ruleBuilder">Regla de validación.</param>
        /// <param name="fieldName">Nombre del campo.</param>
        /// <returns>IRuleBuilder.</returns>
        public static IRuleBuilder<T, string?> IsSubdomain<T>(this IRuleBuilder<T, string?> ruleBuilder, string? fieldName = null)
        => ruleBuilder.Custom((value, context) =>
        {
            if (!value.ToIsNullOrEmpty() && !value.ToIsSubdomain())
            {
                context.AddFailure(Messages.Subdomain(value, fieldName ?? context.DisplayName));
            }
        });

        /// <summary>
        /// Comprueba si el valor ingresado es valido.
        /// </summary>
        /// <typeparam name="T">Entidad a validar.</typeparam>
        /// <param name="ruleBuilder">Regla de validación.</param>
        /// <param name="message">Mensaje a mostrar.</param>
        /// <returns>IRuleBuilder.</returns>
        public static IRuleBuilder<T, string?> IsSubdomainWithMessage<T>(this IRuleBuilder<T, string?> ruleBuilder, string message)
        => ruleBuilder.Custom((value, context) =>
        {
            if (!value.ToIsNullOrEmpty() && !value.ToIsSubdomain())
            {
                context.AddFailure(message);
            }
        });
    }
}
