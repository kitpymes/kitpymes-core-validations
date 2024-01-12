using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Kitpymes.Core.Validations.Extensions;
using Microsoft.Extensions.Hosting;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

/*** Configuración desde el AppSettings para FluentValidator. ***/
builder.Services.LoadValidations();

/*** Configuración manual para FluentValidator. ***/
//builder.Services.LoadValidations(options => options.WithEnabled().WithFluentValidation("Tests.Api.Models"));

var application = builder.Build();

// Configure the HTTP request pipeline.
if (application.Environment.IsDevelopment())
{
    application.UseSwagger();
    application.UseSwaggerUI();
}

application.MapControllers();

/*** Agregamos el middlware para las validaciones. ***/
application.LoadValidations();

await application.RunAsync();
