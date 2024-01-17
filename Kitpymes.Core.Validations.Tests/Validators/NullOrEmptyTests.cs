using Kitpymes.Core.Shared.Util;
using Kitpymes.Core.Validations.Abstractions.Exceptions;
using Kitpymes.Core.Validations.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kitpymes.Core.Validations.Tests.Validators;

[TestClass]
public class NullOrEmptyTests : ValidatorsTestBase
{
    [TestMethod]
    public void IsNullOrEmpty_AddRules_Passing_InvalidArguments_Returns_Errors()
    {
        var result = Validator
            .AddRule(FakeTypes.ColecctionsTypes.List_Null, x => x.IsNullOrEmpty())
            .AddRule(FakeTypes.ReferenceTypes.MatrizTypes.Array_Default, x => x.IsNullOrEmpty(FIELD_NAME))
            .AddRule(FakeTypes.ReferenceTypes.ClassTypes.String_Default, x => x.IsNullOrEmptyWithMessage(MESSAGE))
            .AddRule(FakeTypes.ReferenceTypes.ClassTypes.Object_Null, x => x.IsNullOrEmpty(OTHER_FIELD_NAME).IsNullOrEmptyWithMessage(OTHER_MESSAGE));

        Assert.IsFalse(result.IsValid);

        var exception = Assert.ThrowsException<ValidationsException>(() => result.Throw());

        Assert.IsNotNull(exception);
        Assert.IsTrue(exception.Contains(Messages.NullOrEmpty()));
        Assert.IsTrue(exception.Contains(Messages.NullOrEmpty(FIELD_NAME)));
        Assert.IsTrue(exception.Contains(MESSAGE));
        Assert.IsTrue(exception.Contains(Messages.NullOrEmpty(OTHER_FIELD_NAME)));
        Assert.IsTrue(exception.Contains(OTHER_MESSAGE));
    }
}
