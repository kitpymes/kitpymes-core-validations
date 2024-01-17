using Kitpymes.Core.Validations.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Api
{
    public class Startup()
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            /*** Configuración desde el AppSettings para FluentValidator. ***/
            services.LoadValidations();

            /*** Configuración manual para FluentValidator. ***/
            //services.LoadValidations(validator => validator.WithEnabled().WithFluentValidation("Tests.Api.Models"));
        }

        public void Configure(IApplicationBuilder app)
        {
            /*** Agregamos el middlware para las validaciones. ***/
            app.LoadValidations();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
