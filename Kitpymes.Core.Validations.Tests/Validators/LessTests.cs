using Kitpymes.Core.Validations.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kitpymes.Core.Validations.Tests.Validators;

[TestClass]
public class LessTests : ValidatorsTestBase
{
    private readonly int min = 100;

    [TestMethod]
    public void IsLess_AddRules_Passing_InvalidArguments_Returns_Errors()
    {
        var result = Validator
            .AddRule(99, x => x.IsLess(min))
            .AddRule(44, x => x.IsLess(min, FIELD_NAME))
            .AddRule(0, x => x.IsLessWithMessage(min, MESSAGE))
            .AddRule(-33, x => x.IsLess(min, OTHER_FIELD_NAME).IsLessWithMessage(min, OTHER_MESSAGE));

        Assert.IsFalse(result.IsValid);

        var exception = Assert.ThrowsException<ValidationsException>(() => result.Throw());

        Assert.IsNotNull(exception);
        Assert.IsTrue(exception.Contains(Messages.Less(min)));
        Assert.IsTrue(exception.Contains(Messages.Less(min, FIELD_NAME)));
        Assert.IsTrue(exception.Contains(MESSAGE));
        Assert.IsTrue(exception.Contains(Messages.Less(min, OTHER_FIELD_NAME)));
        Assert.IsTrue(exception.Contains(OTHER_MESSAGE));
    }

    [TestMethod]
    public void IsLessOrEqual_AddRules_Passing_InvalidArguments_Returns_Errors()
    {
        var result = Validator
            .AddRule(100, x => x.IsLessOrEqual(min))
            .AddRule(99, x => x.IsLessOrEqual(min, FIELD_NAME))
            .AddRule(0, x => x.IsLessOrEqualWithMessage(min, MESSAGE))
            .AddRule(-55, x => x.IsLessOrEqual(min, OTHER_FIELD_NAME).IsLessOrEqualWithMessage(min, OTHER_MESSAGE));

        Assert.IsFalse(result.IsValid);

        var exception = Assert.ThrowsException<ValidationsException>(() => result.Throw());

        Assert.IsNotNull(exception);
        Assert.IsTrue(exception.Contains(Messages.LessOrEqual(min)));
        Assert.IsTrue(exception.Contains(Messages.LessOrEqual(min, FIELD_NAME)));
        Assert.IsTrue(exception.Contains(MESSAGE));
        Assert.IsTrue(exception.Contains(Messages.LessOrEqual(min, OTHER_FIELD_NAME)));
        Assert.IsTrue(exception.Contains(OTHER_MESSAGE));
    }
}
