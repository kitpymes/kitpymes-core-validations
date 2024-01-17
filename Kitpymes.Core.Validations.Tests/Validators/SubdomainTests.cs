using Kitpymes.Core.Shared.Util;
using Kitpymes.Core.Validations.Abstractions.Exceptions;
using Kitpymes.Core.Validations.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kitpymes.Core.Validations.Tests.Validators;

[TestClass]
public class SubdomainTests : ValidatorsTestBase
{
    [TestMethod]
    public void Subdomain_AddRules_Passing_InvalidArguments_Returns_Errors()
    {
        var value = FakeTypes.ReferenceTypes.ClassTypes.String_New();

        var result = Validator
            .AddRule(value, x => x.IsSubdomain())
            .AddRule(value, x => x.IsSubdomain(FIELD_NAME))
            .AddRule(FakeTypes.ReferenceTypes.ClassTypes.String_Default, x => x.IsSubdomain())
            .AddRule(FakeTypes.ReferenceTypes.ClassTypes.String_Empty, x => x.IsSubdomain(FIELD_NAME))
            .AddRule(FakeTypes.ReferenceTypes.ClassTypes.String_Empty, x => x.IsSubdomainWithMessage(MESSAGE))
            .AddRule(FakeTypes.ReferenceTypes.ClassTypes.String_Null, x => x.IsSubdomain(OTHER_FIELD_NAME).IsSubdomainWithMessage(OTHER_MESSAGE));

        Assert.IsFalse(result.IsValid);

        var exception = Assert.ThrowsException<ValidationsException>(() => result.Throw());

        Assert.IsNotNull(exception);
        Assert.IsTrue(exception.Contains(Messages.Subdomain(value)));
        Assert.IsTrue(exception.Contains(Messages.Subdomain(value, FIELD_NAME)));
        Assert.IsTrue(exception.Contains(Messages.Subdomain()));
        Assert.IsTrue(exception.Contains(Messages.Subdomain(fieldName: FIELD_NAME)));
        Assert.IsTrue(exception.Contains(MESSAGE));
        Assert.IsTrue(exception.Contains(Messages.Subdomain(fieldName: OTHER_FIELD_NAME)));
        Assert.IsTrue(exception.Contains(OTHER_MESSAGE));
    }
}
