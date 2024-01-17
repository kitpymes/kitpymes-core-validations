using Kitpymes.Core.Shared.Util;
using Kitpymes.Core.Validations.Abstractions.Exceptions;
using Kitpymes.Core.Validations.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kitpymes.Core.Validations.Tests.Validators;

[TestClass]
public class FileExtensionTests : ValidatorsTestBase
{
    [TestMethod]
    public void FileExtension_AddRules_Passing_InvalidArguments_Returns_Errors()
    {
        var value = FakeTypes.ReferenceTypes.ClassTypes.String_New();

        var result = Validator
            .AddRule(value, x => x.IsFileExtension())
            .AddRule(value, x => x.IsFileExtension(FIELD_NAME))
            .AddRule(FakeTypes.ReferenceTypes.ClassTypes.String_Default, x => x.IsFileExtension())
            .AddRule(FakeTypes.ReferenceTypes.ClassTypes.String_Empty, x => x.IsFileExtension(FIELD_NAME))
            .AddRule(FakeTypes.ReferenceTypes.ClassTypes.String_Empty, x => x.IsFileExtensionWithMessage(MESSAGE))
            .AddRule(FakeTypes.ReferenceTypes.ClassTypes.String_Null, x => x.IsFileExtension(OTHER_FIELD_NAME).IsFileExtensionWithMessage(OTHER_MESSAGE));

        Assert.IsFalse(result.IsValid);

        var exception = Assert.ThrowsException<ValidationsException>(() => result.Throw());

        Assert.IsNotNull(exception);
        Assert.IsTrue(exception.Contains(Messages.FileExtension(value)));
        Assert.IsTrue(exception.Contains(Messages.FileExtension(value, FIELD_NAME)));
        Assert.IsTrue(exception.Contains(Messages.FileExtension()));
        Assert.IsTrue(exception.Contains(Messages.FileExtension(fieldName: FIELD_NAME)));
        Assert.IsTrue(exception.Contains(MESSAGE));
        Assert.IsTrue(exception.Contains(Messages.FileExtension(fieldName: OTHER_FIELD_NAME)));
        Assert.IsTrue(exception.Contains(OTHER_MESSAGE));
    }
}
