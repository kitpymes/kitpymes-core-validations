using FluentValidation;
using Kitpymes.Core.Validations.FluentValidation.Extensions;

namespace Tests.Api.Models
{
    public class PersonAddDtoValidator : AbstractValidator<PersonAddDto>
    {
        public PersonAddDtoValidator()
        {
            RuleFor(x => x.Age).IsOutRange(0, 10, "Edad");
            RuleFor(x => x.Name).IsName("Tenant").IsLess(100);
            RuleFor(x => x.Email).IsEmailWithMessage("El correo eléctronico tiene un formato incorrecto.");
        }
    }
}
