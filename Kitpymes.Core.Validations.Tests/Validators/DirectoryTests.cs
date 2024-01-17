using Kitpymes.Core.Shared.Util;
using Kitpymes.Core.Validations.Abstractions.Exceptions;
using Kitpymes.Core.Validations.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kitpymes.Core.Validations.Tests.Validators;

[TestClass]
public class DirectoryTests : ValidatorsTestBase
{
    [TestMethod]
    public void IsDirectoryAddRules_Passing_InvalidArguments_Returns_Errors()
    {
        var value = FakeTypes.ReferenceTypes.ClassTypes.String_New();

        var result = Validator
            .AddRule(value, x => x.IsDirectory())
            .AddRule(value, x => x.IsDirectory(FIELD_NAME))
            .AddRule(FakeTypes.ReferenceTypes.ClassTypes.String_Default, x => x.IsDirectory())
            .AddRule(FakeTypes.ReferenceTypes.ClassTypes.String_Empty, x => x.IsDirectory(FIELD_NAME))
            .AddRule(FakeTypes.ReferenceTypes.ClassTypes.String_Empty, x => x.IsDirectoryWithMessage(MESSAGE))
            .AddRule(FakeTypes.ReferenceTypes.ClassTypes.String_Null, x => x.IsDirectory(OTHER_FIELD_NAME).IsDirectoryWithMessage(OTHER_MESSAGE));

        Assert.IsFalse(result.IsValid);

        var exception = Assert.ThrowsException<ValidationsException>(() => result.Throw());

        Assert.IsNotNull(exception);
        Assert.IsTrue(exception.Contains(Messages.Directory(value)));
        Assert.IsTrue(exception.Contains(Messages.Directory(value, FIELD_NAME)));
        Assert.IsTrue(exception.Contains(Messages.Directory()));
        Assert.IsTrue(exception.Contains(Messages.Directory(fieldName: FIELD_NAME)));
        Assert.IsTrue(exception.Contains(MESSAGE));
        Assert.IsTrue(exception.Contains(Messages.Directory(fieldName: OTHER_FIELD_NAME)));
        Assert.IsTrue(exception.Contains(OTHER_MESSAGE));
    }
}
