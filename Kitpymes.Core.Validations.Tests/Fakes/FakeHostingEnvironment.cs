using Microsoft.Extensions.Hosting;
using Moq;

namespace Kitpymes.Core.Validations.Tests.Fakes
{
    public class FakeHostingEnvironment
    {
        public static IHostEnvironment Get(string? enviromentName)
        {
            var environment = new Mock<IHostEnvironment>();

            if (!string.IsNullOrWhiteSpace(enviromentName))
            {
                environment
                    .Setup(e => e.EnvironmentName)
                    .Returns(enviromentName);
            }

            return environment.Object;
        }
    }
}
