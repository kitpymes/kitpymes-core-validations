using Tests.Api.Models;
using Kitpymes.Core.Shared.Extensions;
using Kitpymes.Core.Validations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Microsoft.AspNetCore.Mvc.Testing;
using Kitpymes.Core.Validations.Extensions;
using Kitpymes.Core.Validations.Abstractions.Exceptions;

namespace Tests.Api.EndpointsTests
{
    [TestClass]
    public class ValidatorTests
    {
        [TestMethod]
        public async Task Validate_Endpoint_AddPerson()
        {
            using var host = await new HostBuilder()
                .ConfigureAppConfiguration(configHost =>
                {
                    configHost.SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile($"appsettings.Development.json", optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables();
                })
                .UseEnvironment(Environments.Development)
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder
                        .UseTestServer()
                        .ConfigureServices(services =>
                        {
                            services.AddControllers()
                                .AddJsonOptions(options => options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);

                            /*** Configuración desde el AppSettings para FluentValidator. ***/
                            services.LoadValidations();

                            /*** Configuración manual para FluentValidator. ***/
                            //services.LoadValidations(options => options.WithEnabled().WithFluentValidation("Tests.Api.Models"));
                        })
                        .Configure(application =>
                        {
                            /*** Agregamos el middlware para las validaciones. ***/
                            application.LoadValidations();
                        })
                        .UseTestServer()
                        .UseStartup<Startup>();
                })
                .StartAsync();       

            var client = host.GetTestClient();

            var uri = "/Validator/AddPerson";

            var person = new
            {
                Age = 15,
                Email = "ddd@@_.45r",
                Name = "a@_"
            };

            var stringContent = new StringContent(person.ToSerialize(), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.PostAsync(uri, stringContent);
            var httpResponseContent = await httpResponse.Content.ReadAsStringAsync();

            // Assert
            Assert.IsFalse(string.IsNullOrWhiteSpace(httpResponseContent));

            Log(httpResponseContent);
        }

        [TestMethod]
        public void Validate_Domain_AddPerson()
        {
            // Arrange
            var age = 15;
            var name = "";
            var email = "ddd@@_.45r";

            // Act
            var exception = Assert.ThrowsException<ValidationsException>(() => new Person(age, name, email));

            // Assert
            Assert.IsNotNull(exception);
            Assert.IsTrue(exception.HasErrors);

            Log(exception.Message);
        }

        [TestMethod]
        public void Validate_Domain_ChangeName()
        {
            // Arrange
            var invalidName = "435_///3sdff·543";
            var person = new Person(20, "Pedro", "pedro@gmail.com");

            // Act
            var exception = Assert.ThrowsException<ValidationsException>(() => person.ChangeName(invalidName));

            // Assert
            Assert.IsNotNull(exception);
            Assert.IsTrue(exception.HasErrors);

            Log(exception.Message);
        }

        private void Log(string message) => Console.WriteLine(message);
    }
}
