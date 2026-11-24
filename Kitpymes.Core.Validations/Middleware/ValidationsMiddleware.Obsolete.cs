// -----------------------------------------------------------------------
// <copyright file="ValidationsMiddleware.Obsolete.cs" company="Kitpymes">
// Copyright (c) Kitpymes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project docs folder for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Kitpymes.Core.Validations.Middleware;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Kitpymes.Core.Shared.Extensions;
using Kitpymes.Core.Logger.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AppResult = Results;
using Kitpymes.Core.Shared;
using Kitpymes.Core.Validations.Abstractions.Exceptions;

/*
    Clase del middlware ValidationsMiddleware
    Contiene el mensaje de devolución de los errores de validaciones
*/

/// <summary>
/// Clase del middlware <c>ValidationsMiddleware</c>.
/// Contiene el mensaje de devolución de los errores de validaciones.
/// </summary>
/// <remarks>
/// <para>En esta clase se pueden agregar todas las propiedades que se requieran para las validaciones.</para>
/// </remarks>
/// <remarks>
/// Inicializa una nueva instancia de la clase <see cref="ValidationsMiddleware"/>.
/// </remarks>
/// <param name="requestDelegate">Una función que puede procesar una solicitud HTTP.</param>
[Obsolete("API antigua. La validación va en el paso del mediador: LoadValidations(o => o.WithAssemblies(...)). Se elimina en 3.0.0.", DiagnosticId = "KPY0007")]
[EditorBrowsable(EditorBrowsableState.Never)]
public class ValidationsMiddleware(RequestDelegate requestDelegate)
{
    private RequestDelegate RequestDelegate => requestDelegate;

    private ILogger? Logger { get; set; }

    private string? RequestBody { get; set; }

    /// <summary>
    /// Devuelve el mensaje configurado al cliente si ocurre una excepción.
    /// </summary>
    /// <param name="httpContext">Encapsula toda la información específica de una solicitud HTTP.</param>
    /// <returns>Task.</returns>
    public async Task InvokeAsync(HttpContext httpContext)
    {
        if (httpContext != null)
        {
            try
            {
                Logger = httpContext.RequestServices.GetService<ILoggerService>()?.CreateLogger<ValidationsMiddleware>();

                await ReadRequestBodyAsync(httpContext).ConfigureAwait(false);

                await RequestDelegate(httpContext).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                await HandleExceptionAsync(httpContext, exception).ConfigureAwait(false);
            }
        }
    }

    private async Task ReadRequestBodyAsync(HttpContext httpContext)
    {
        // Still enable buffering before anything reads
        httpContext.Request.EnableBuffering();

        using var stringReader = new StreamReader(httpContext.Request.Body, leaveOpen: true);

        RequestBody = await stringReader.ReadToEndAsync();

        // Reset the request body stream position to the start so we can read it
        httpContext.Request.Body.Position = 0;
    }

    private async Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
    {
        var environment = httpContext.RequestServices.ToHostEnvironment();

        var optionalData = new Dictionary<string, IEnumerable<object>>();

        if (!RequestBody.ToIsNullOrEmpty())
        {
            var request = RequestBody.Split("\n");

            optionalData.ToAddOrUpdate(nameof(RequestBody), request);
        }

        if (!exception.StackTrace.ToIsNullOrEmpty())
        {
            optionalData.ToAddOrUpdate(nameof(exception.StackTrace), exception.StackTrace.Split("\r\n"));
        }

        var details = httpContext.ToDetails(optionalData);

        AppResult.IApiResultError? result = null;

        var exceptionTypeName = exception.GetType().Name;

        HttpStatusCode statusCode = default;

        switch (exception)
        {
            case ValidationsException validationException when exception is ValidationsException:

                statusCode = HttpStatusCode.BadRequest;

                result = AppResult.ApiResult.Error(options =>
                {
                    options.WithTitle(SharedResource.MsgErrorsTitle)
                        .WithStatusCode(statusCode)
                        .WithMessage(validationException.Message)
                        .WithErrors(validationException.Errors);

                    if (environment?.IsDevelopment() == true)
                    {
                        options
                            .WithExceptionType(exceptionTypeName)
                            .WithDetails(details);
                    }
                });

                break;

            case UnauthorizedAccessException unauthorizedAccessException when exception is UnauthorizedAccessException:

                statusCode = HttpStatusCode.Unauthorized;

                result = AppResult.ApiResult.Error(options =>
                {
                    options
                        .WithTitle(SharedResource.MsgErrorsTitle)
                        .WithStatusCode(statusCode);

                    if (environment?.IsDevelopment() == true)
                    {
                        options
                            .WithMessage(unauthorizedAccessException.ToFullMessage())
                            .WithExceptionType(exceptionTypeName)
                            .WithDetails(details);
                    }
                    else
                    {
                        Logger?.Error(exception);

                        options.WithMessage(SharedResource.MsgUnauthorizedAccess);
                    }
                });

                break;

            default:

                statusCode = HttpStatusCode.InternalServerError;

                result = AppResult.ApiResult.Error(options =>
                {
                    options
                        .WithTitle(SharedResource.MsgErrorsTitle)
                        .WithStatusCode(statusCode);

                    if (environment?.IsDevelopment() == true)
                    {
                        options
                            .WithMessage(exception.ToFullMessage())
                            .WithExceptionType(exceptionTypeName)
                            .WithDetails(details);
                    }
                    else
                    {
                        Logger?.Error(exception);

                        options.WithMessage(SharedResource.MsgFriendlyUnexpectedError);
                    }
                });

                break;
        }

        await httpContext.Response.ToResultAsync(
            status: statusCode,
            message: result.ToSerialize(x => x.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull),
            headers: new Dictionary<string, IEnumerable<string>>
            {
                { nameof(Exception), new List<string> { exceptionTypeName } },
            }).ConfigureAwait(false);
    }
}
