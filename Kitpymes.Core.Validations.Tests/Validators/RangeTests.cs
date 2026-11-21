using Kitpymes.Core.Shared.Util;
using Kitpymes.Core.Validations.Abstractions.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kitpymes.Core.Validations.Tests.Validators;

[TestClass]
public class RangeTests : ValidatorsTestBase
{
    #region Inside

    [TestMethod]
    public void IsInsideRange_AddRules_Passing_InvalidArguments_Returns_Errors()
    {
        var min = 10;
        var max = 20;

        var result = Validator
            .AddRule(11, x => x.IsInsideRange(min, max))
            .AddRule(12, x => x.IsInsideRange(min, max, FIELD_NAME))
            .AddRule(18, x => x.IsInsideRangeWithMessage(min, max, MESSAGE))
            .AddRule(19, x => x.IsInsideRange(min, max, OTHER_FIELD_NAME).IsInsideRangeWithMessage(min, max, OTHER_MESSAGE));

        Assert.IsFalse(result.IsValid);

        var exception = Assert.ThrowsExactly<ValidationsException>(() => result.Throw());

        Assert.IsNotNull(exception);
        Assert.IsTrue(exception.Contains(Messages.InsideRange(min, max)));
        Assert.IsTrue(exception.Contains(Messages.InsideRange(min, max, FIELD_NAME)));
        Assert.IsTrue(exception.Contains(MESSAGE));
        Assert.IsTrue(exception.Contains(Messages.InsideRange(min, max, OTHER_FIELD_NAME)));
        Assert.IsTrue(exception.Contains(OTHER_MESSAGE));
    }

    [TestMethod]
    public void IsInsideOrEqualRange_AddRules_Passing_InvalidArguments_Returns_Errors()
    {
        var min = 10;
        var max = 20;

        var result = Validator
            .AddRule(10, x => x.IsInsideOrEqualRange(min, max))
            .AddRule(11, x => x.IsInsideOrEqualRange(min, max, FIELD_NAME))
            .AddRule(19, x => x.IsInsideOrEqualRangeWithMessage(min, max, MESSAGE))
            .AddRule(20, x => x.IsInsideOrEqualRange(min, max, OTHER_FIELD_NAME).IsInsideOrEqualRangeWithMessage(min, max, OTHER_MESSAGE));

        Assert.IsFalse(result.IsValid);

        var exception = Assert.ThrowsExactly<ValidationsException>(() => result.Throw());

        Assert.IsNotNull(exception);
        Assert.IsTrue(exception.Contains(Messages.InsideOrEqualRange(min, max)));
        Assert.IsTrue(exception.Contains(Messages.InsideOrEqualRange(min, max, FIELD_NAME)));
        Assert.IsTrue(exception.Contains(MESSAGE));
        Assert.IsTrue(exception.Contains(Messages.InsideOrEqualRange(min, max, OTHER_FIELD_NAME)));
        Assert.IsTrue(exception.Contains(OTHER_MESSAGE));
    }

    #endregion Inside

    #region Out

    [TestMethod]
    public void IsOutRange_AddRules_Passing_InvalidArguments_Returns_Errors()
    {
        var min = 10;
        var max = 20;

        var result = Validator
            .AddRule(8, x => x.IsOutRange(min, max))
            .AddRule(9, x => x.IsOutRange(min, max, FIELD_NAME))
            .AddRule(21, x => x.IsOutRangeWithMessage(min, max, MESSAGE))
            .AddRule(22, x => x.IsOutRange(min, max, OTHER_FIELD_NAME).IsOutRangeWithMessage(min, max, OTHER_MESSAGE));

        Assert.IsFalse(result.IsValid);

        var exception = Assert.ThrowsExactly<ValidationsException>(() => result.Throw());

        Assert.IsNotNull(exception);
        Assert.IsTrue(exception.Contains(Messages.OutRange(min, max)));
        Assert.IsTrue(exception.Contains(Messages.OutRange(min, max, FIELD_NAME)));
        Assert.IsTrue(exception.Contains(MESSAGE));
        Assert.IsTrue(exception.Contains(Messages.OutRange(min, max, OTHER_FIELD_NAME)));
        Assert.IsTrue(exception.Contains(OTHER_MESSAGE));
    }

    [TestMethod]
    public void IsOutOrEqualRange_AddRules_Passing_InvalidArguments_Returns_Errors()
    {
        var min = 10;
        var max = 20;

        var result = Validator
            .AddRule(9, x => x.IsOutOrEqualRange(min, max))
            .AddRule(10, x => x.IsOutOrEqualRange(min, max, FIELD_NAME))
            .AddRule(20, x => x.IsOutOrEqualRangeWithMessage(min, max, MESSAGE))
            .AddRule(21, x => x.IsOutOrEqualRange(min, max, OTHER_FIELD_NAME).IsOutOrEqualRangeWithMessage(min, max, OTHER_MESSAGE));

        Assert.IsFalse(result.IsValid);

        var exception = Assert.ThrowsExactly<ValidationsException>(() => result.Throw());

        Assert.IsNotNull(exception);
        Assert.IsTrue(exception.Contains(Messages.OutOrEqualRange(min, max)));
        Assert.IsTrue(exception.Contains(Messages.OutOrEqualRange(min, max, FIELD_NAME)));
        Assert.IsTrue(exception.Contains(MESSAGE));
        Assert.IsTrue(exception.Contains(Messages.OutOrEqualRange(min, max, OTHER_FIELD_NAME)));
        Assert.IsTrue(exception.Contains(OTHER_MESSAGE));
    }

    #endregion Out
}
