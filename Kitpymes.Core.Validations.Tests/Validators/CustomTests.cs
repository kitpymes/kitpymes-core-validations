using Kitpymes.Core.Validations.Abstractions.Exceptions;
using Kitpymes.Core.Validations.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kitpymes.Core.Validations.Tests.Validators;

[TestClass]
public class CustomTests : ValidatorsTestBase
{
    [TestMethod]
    public void IsCustom_AddRules_Passing_InvalidArguments_Returns_Errors()
    {
        var result = Validator
            .AddRule(() => FakeTypes.ColecctionsTypes.List_Null is null, MESSAGE)
            .AddRule(() =>
            {
                if (FakeTypes.ColecctionsTypes.Stack_CountZero.Count == 0)
                {
                    return FakeTypes.ColecctionsTypes.List_Null is null;
                }

                return false;
            }, OTHER_MESSAGE);

        Assert.IsFalse(result.IsValid);

        var exception = Assert.ThrowsException<ValidationsException>(() => result.Throw());

        Assert.IsTrue(exception.Contains(MESSAGE));
        Assert.IsTrue(exception.Contains(OTHER_MESSAGE));
    }
}
