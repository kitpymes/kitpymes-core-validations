using Kitpymes.Core.Shared.Util;
using Kitpymes.Core.Validations.Abstractions.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kitpymes.Core.Validations.Tests.Validators;

[TestClass]
public class GreaterTests : ValidatorsTestBase
{
    private readonly int max = 100;

    [TestMethod]
    public void IsGreater_AddRules_Passing_InvalidArguments_Returns_Errors()
    {
        var result = Validator
            .AddRule(101, x => x.IsGreater(max))
            .AddRule(102, x => x.IsGreater(max, FIELD_NAME))
            .AddRule(1000, x => x.IsGreaterWithMessage(max, MESSAGE))
            .AddRule(232323, x => x.IsGreater(max, OTHER_FIELD_NAME).IsGreaterWithMessage(max, OTHER_MESSAGE));

        Assert.IsFalse(result.IsValid);

        var exception = Assert.ThrowsExactly<ValidationsException>(() => result.Throw());

        Assert.IsNotNull(exception);
        Assert.IsTrue(exception.Contains(Messages.Greater(max)));
        Assert.IsTrue(exception.Contains(Messages.Greater(max, FIELD_NAME)));
        Assert.IsTrue(exception.Contains(MESSAGE));
        Assert.IsTrue(exception.Contains(Messages.Greater(max, OTHER_FIELD_NAME)));
        Assert.IsTrue(exception.Contains(OTHER_MESSAGE));
    }

    [TestMethod]
    public void IsGreaterOrEqual_AddRules_Passing_InvalidArguments_Returns_Errors()
    {
        var result = Validator
            .AddRule(100, x => x.IsGreaterOrEqual(max))
            .AddRule(102, x => x.IsGreaterOrEqual(max, FIELD_NAME))
            .AddRule(1000, x => x.IsGreaterOrEqualWithMessage(max, MESSAGE))
            .AddRule(232323, x => x.IsGreaterOrEqual(max, OTHER_FIELD_NAME).IsGreaterOrEqualWithMessage(max, OTHER_MESSAGE));

        Assert.IsFalse(result.IsValid);

        var exception = Assert.ThrowsExactly<ValidationsException>(() => result.Throw());

        Assert.IsNotNull(exception);
        Assert.IsTrue(exception.Contains(Messages.GreaterOrEqual(max)));
        Assert.IsTrue(exception.Contains(Messages.GreaterOrEqual(max, FIELD_NAME)));
        Assert.IsTrue(exception.Contains(MESSAGE));
        Assert.IsTrue(exception.Contains(Messages.GreaterOrEqual(max, OTHER_FIELD_NAME)));
        Assert.IsTrue(exception.Contains(OTHER_MESSAGE));
    }
}
