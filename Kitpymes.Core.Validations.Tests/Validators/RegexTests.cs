using Kitpymes.Core.Shared.Util;
using Kitpymes.Core.Validations.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kitpymes.Core.Validations.Tests.Validators;

[TestClass]
public class RegexTests : ValidatorsTestBase
{
    [TestMethod]
    public void IsRegex_AddRules_Passing_InvalidArguments_Returns_Errors()
    {
        var regex = Regexp.ForEmail;

        var result = Validator
            .AddRule(FakeTypes.ColecctionsTypes.List_Null, x => x.IsRegex(regex))
            .AddRule(FakeTypes.ReferenceTypes.MatrizTypes.Array_Default, x => x.IsRegex(regex, FIELD_NAME))
            .AddRule(FakeTypes.ColecctionsTypes.Stack_CountZero, x => x.IsRegexWithMessage(regex, MESSAGE))
            .AddRule(FakeTypes.ColecctionsTypes.ArrayClass_CountZero, x => x.IsRegex(regex, OTHER_FIELD_NAME).IsRegexWithMessage(regex, OTHER_MESSAGE));

        Assert.IsFalse(result.IsValid);

        var exception = Assert.ThrowsException<ValidationsException>(() => result.Throw());

        Assert.IsNotNull(exception);
        Assert.IsTrue(exception.Contains(Messages.Regex()));
        Assert.IsTrue(exception.Contains(Messages.Regex(FIELD_NAME)));
        Assert.IsTrue(exception.Contains(MESSAGE));
        Assert.IsTrue(exception.Contains(Messages.Regex(OTHER_FIELD_NAME)));
        Assert.IsTrue(exception.Contains(OTHER_MESSAGE));
    }
}
