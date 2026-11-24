using System;
using System.Threading.Tasks;
using Kitpymes.Core.Shared.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kitpymes.Core.Validations.Tests.Rules;

/// <summary>
/// Las expresiones de Email, Name y Subdomain están copiadas de Kitpymes.Core.Shared: dan lo mismo que Shared.
/// </summary>
[TestClass]
public class PatternsTests
{
    private static readonly string[] Samples =
    {
        "ana@kitpymes.com", "ana.b-c@dominio.es", "ana@dominio", "ana@dominio.comercial", "@dominio.com", "ana@@x.com",
        "José Ñúñez", "Ana1", "ana_b", "mi-sub.dominio_1", "sub dominio", "ÅøØ", "x", "a.b", "a@b.co.uk", "ñ@ñ.es",
    };

    [TestMethod]
    public async Task Same_Result_As_Shared()
    {
        var email = new TestValidator<Order>(v => v.For(x => x.Name).Email());
        var name = new TestValidator<Order>(v => v.For(x => x.Name).Name());
        var subdomain = new TestValidator<Order>(v => v.For(x => x.Name).Subdomain());

        foreach (var sample in Samples)
        {
            var order = new Order { Name = sample };

            Assert.AreEqual(sample.ToIsEmail(), (await email.ValidateAsync(order)).IsSuccess, "Email: " + sample);
            Assert.AreEqual(sample.ToIsName(), (await name.ValidateAsync(order)).IsSuccess, "Name: " + sample);
            Assert.AreEqual(sample.ToIsSubdomain(), (await subdomain.ValidateAsync(order)).IsSuccess, "Subdomain: " + sample);
        }
    }
}
