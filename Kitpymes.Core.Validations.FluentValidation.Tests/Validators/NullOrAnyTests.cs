using Kitpymes.Core.Shared.Util;
using Kitpymes.Core.Validations.FluentValidation.Extensions;
using Kitpymes.Core.Validations.FluentValidation.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kitpymes.Core.Validations.FluentValidation.Tests.Validators;

[TestClass]
public class NullOrAnyTests : ValidatorsTestBase
{
    [TestMethod]
    public void IsNullOrAny_Passing_InvalidArguments_Returns_Errors()
    {
        var mock = new FakeObject { Roles = [] };

        var validator = new FakeObjectValidator();

        validator.RuleFor(x => x.Roles)
            .IsNullOrAny()
                .IsNullOrAny(FIELD_NAME)
                    .IsNullOrAnyWithMessage(MESSAGE);

        var result = validator.Validate(mock);

        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Count == 3);

        Assert.IsTrue(result.Contains(Messages.NullOrAny(nameof(FakeObject.Roles))));
        Assert.IsTrue(result.Contains(Messages.NullOrAny(FIELD_NAME)));
        Assert.IsTrue(result.Contains(MESSAGE));

        var messages = result.ToString();
        
        Assert.IsTrue(messages.Contains(Messages.NullOrAny(nameof(FakeObject.Roles))));
        Assert.IsTrue(messages.Contains(Messages.NullOrAny(FIELD_NAME)));
        Assert.IsTrue(messages.Contains(MESSAGE));
    }
}