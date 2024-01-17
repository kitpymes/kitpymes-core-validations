using Kitpymes.Core.Shared.Util;
using Kitpymes.Core.Validations.Abstractions.Exceptions;
using Kitpymes.Core.Validations.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kitpymes.Core.Validations.Tests.Validators;

[TestClass]
public class EqualTests : ValidatorsTestBase
{
    private const string FIELD_NAME_COMPARE = nameof(FIELD_NAME_COMPARE);

    [TestMethod]
    public void IsEqual_AddRules_Passing_InvalidArguments_Returns_Errors()
    {
        var result = Validator
            .AddRule(FakeTypes.ReferenceTypes.ClassTypes.Class_New(), x => x.IsEqual(FakeTypes.ReferenceTypes.ClassTypes.Class_New()))
            .AddRule("a", x => x.IsEqual("a", FIELD_NAME))
            .AddRule(1, x => x.IsEqual(1, OTHER_FIELD_NAME, FIELD_NAME_COMPARE))
            .AddRule("a", x => x.IsEqualWithMessage("A", MESSAGE))
            ;

        Assert.IsFalse(result.IsValid);

        var exception = Assert.ThrowsException<ValidationsException>(() => result.Throw());

        Assert.IsNotNull(exception);
        Assert.IsTrue(exception.Contains(Messages.Equal()));
        Assert.IsTrue(exception.Contains(Messages.Equal(FIELD_NAME)));
        Assert.IsTrue(exception.Contains(Messages.Equal(OTHER_FIELD_NAME, FIELD_NAME_COMPARE)));
        Assert.IsTrue(exception.Contains(MESSAGE));
    }

    [TestMethod]
    public void IsNotEqual_AddRules_Passing_InvalidArguments_Returns_Errors()
    {
        var result = Validator
            .AddRule(FakeTypes.ReferenceTypes.ClassTypes.Class_New(), x => x.IsNotEqual(FakeTypes.ReferenceTypes.ClassTypes.Class_New()))
            .AddRule("a", x => x.IsNotEqual("b", FIELD_NAME))
            .AddRule(1, x => x.IsNotEqual(2, OTHER_FIELD_NAME, FIELD_NAME_COMPARE))
            .AddRule("a", x => x.IsNotEqualWithMessage("A", MESSAGE))
            ;

        Assert.IsFalse(result.IsValid);

        var exception = Assert.ThrowsException<ValidationsException>(() => result.Throw());

        Assert.IsNotNull(exception);
        Assert.IsTrue(exception.Contains(Messages.Equal()));
        Assert.IsTrue(exception.Contains(Messages.Equal(FIELD_NAME)));
        Assert.IsTrue(exception.Contains(Messages.Equal(OTHER_FIELD_NAME, FIELD_NAME_COMPARE)));
        Assert.IsTrue(exception.Contains(MESSAGE));
    }
}
