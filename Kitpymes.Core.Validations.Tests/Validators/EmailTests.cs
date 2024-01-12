using Kitpymes.Core.Validations.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kitpymes.Core.Validations.Tests.Validators;

[TestClass]
public class EmailTests : ValidatorsTestBase
{
    [TestMethod]
    public void Email_AddRules_Passing_InvalidArguments_Returns_Errors()
    {
        var value = FakeTypes.ReferenceTypes.ClassTypes.String_New(10, 'X');

        var result = Validator
            .AddRule(value, x => x.IsEmail())
            .AddRule(value, x => x.IsEmail(FIELD_NAME))
            .AddRule(FakeTypes.ReferenceTypes.ClassTypes.String_Default, x => x.IsEmail())
            .AddRule(FakeTypes.ReferenceTypes.ClassTypes.String_Default, x => x.IsEmail(FIELD_NAME))
            .AddRule(FakeTypes.ReferenceTypes.ClassTypes.String_Empty, x => x.IsEmailWithMessage(MESSAGE))
            .AddRule(FakeTypes.ReferenceTypes.ClassTypes.String_Null, x => x.IsEmail(OTHER_FIELD_NAME).IsEmailWithMessage(OTHER_MESSAGE));

        Assert.IsFalse(result.IsValid);

        var exception = Assert.ThrowsException<ValidationsException>(() => result.Throw());

        Assert.IsNotNull(exception);
        Assert.IsTrue(exception.Contains(Messages.Email(value)));
        Assert.IsTrue(exception.Contains(Messages.Email(value, FIELD_NAME)));
        Assert.IsTrue(exception.Contains(Messages.Email()));
        Assert.IsTrue(exception.Contains(Messages.Email(fieldName: FIELD_NAME)));
        Assert.IsTrue(exception.Contains(MESSAGE));
        Assert.IsTrue(exception.Contains(Messages.Email(fieldName: OTHER_FIELD_NAME)));
        Assert.IsTrue(exception.Contains(OTHER_MESSAGE));
    }
}
