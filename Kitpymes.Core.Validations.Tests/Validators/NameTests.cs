using Kitpymes.Core.Shared.Util;
using Kitpymes.Core.Validations.Abstractions.Exceptions;
using Kitpymes.Core.Validations.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kitpymes.Core.Validations.Tests.Validators;

[TestClass]
public class NameTests : ValidatorsTestBase
{
    [TestMethod]
    public void Name_AddRules_Passing_InvalidArguments_Returns_Errors()
    {
        var value = FakeTypes.ReferenceTypes.ClassTypes.String_New();

        var result = Validator
            .AddRule(value, x => x.IsName())
            .AddRule(value, x => x.IsName(FIELD_NAME))
            .AddRule(FakeTypes.ReferenceTypes.ClassTypes.String_Default, x => x.IsName())
            .AddRule(FakeTypes.ReferenceTypes.ClassTypes.String_Empty, x => x.IsName(FIELD_NAME))
            .AddRule(FakeTypes.ReferenceTypes.ClassTypes.String_Empty, x => x.IsNameWithMessage(MESSAGE))
            .AddRule(FakeTypes.ReferenceTypes.ClassTypes.String_Null, x => x.IsName(OTHER_FIELD_NAME).IsNameWithMessage(OTHER_MESSAGE));

        Assert.IsFalse(result.IsValid);

        var exception = Assert.ThrowsException<ValidationsException>(() => result.Throw());

        Assert.IsNotNull(exception);
        Assert.IsTrue(exception.Contains(Messages.Name(value)));
        Assert.IsTrue(exception.Contains(Messages.Name(value, FIELD_NAME)));
        Assert.IsTrue(exception.Contains(Messages.Name()));
        Assert.IsTrue(exception.Contains(Messages.Name(fieldName: FIELD_NAME)));
        Assert.IsTrue(exception.Contains(MESSAGE));
        Assert.IsTrue(exception.Contains(Messages.Name(fieldName: OTHER_FIELD_NAME)));
        Assert.IsTrue(exception.Contains(OTHER_MESSAGE));
    }
}
