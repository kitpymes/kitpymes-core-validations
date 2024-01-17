using Kitpymes.Core.Shared.Util;
using Kitpymes.Core.Validations.Abstractions.Exceptions;
using Kitpymes.Core.Validations.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kitpymes.Core.Validations.Tests.Validators;

[TestClass]
public class NullOrAnyTests : ValidatorsTestBase
{
    [TestMethod]
    public void IsNullOrAny_AddRules_Passing_InvalidArguments_Returns_Errors()
    {
        var result = Validator
            .AddRule(FakeTypes.ColecctionsTypes.List_Null, x => x.IsNullOrAny())
            .AddRule(FakeTypes.ReferenceTypes.MatrizTypes.Array_Default, x => x.IsNullOrAny(FIELD_NAME))
            .AddRule(FakeTypes.ColecctionsTypes.Stack_CountZero, x => x.IsNullOrAnyWithMessage(MESSAGE))
            .AddRule(FakeTypes.ColecctionsTypes.ArrayClass_CountZero, x => x.IsNullOrAny(OTHER_FIELD_NAME).IsNullOrAnyWithMessage(OTHER_MESSAGE));

        Assert.IsFalse(result.IsValid);

        var exception = Assert.ThrowsException<ValidationsException>(() => result.Throw());

        Assert.IsNotNull(exception);
        Assert.IsTrue(exception.Contains(Messages.NullOrAny()));
        Assert.IsTrue(exception.Contains(Messages.NullOrAny(FIELD_NAME)));
        Assert.IsTrue(exception.Contains(MESSAGE));
        Assert.IsTrue(exception.Contains(Messages.NullOrAny(OTHER_FIELD_NAME)));
        Assert.IsTrue(exception.Contains(OTHER_MESSAGE));
    }
}
