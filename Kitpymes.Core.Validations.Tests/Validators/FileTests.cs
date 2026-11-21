using Kitpymes.Core.Shared.Util;
using Kitpymes.Core.Validations.Abstractions.Exceptions;
using Kitpymes.Core.Validations.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kitpymes.Core.Validations.Tests.Validators;

[TestClass]
public class FileTests : ValidatorsTestBase
{
    [TestMethod]
    public void File_AddRules_Passing_InvalidArguments_Returns_Errors()
    {
        var value = FakeTypes.ReferenceTypes.ClassTypes.String_New();

        var result = Validator
            .AddRule(value, x => x.IsFile())
            .AddRule(value, x => x.IsFile(FIELD_NAME))
            .AddRule(FakeTypes.ReferenceTypes.ClassTypes.String_Default, x => x.IsFile())
            .AddRule(FakeTypes.ReferenceTypes.ClassTypes.String_Empty, x => x.IsFile(FIELD_NAME))
            .AddRule(FakeTypes.ReferenceTypes.ClassTypes.String_Empty, x => x.IsFileWithMessage(MESSAGE))
            .AddRule(FakeTypes.ReferenceTypes.ClassTypes.String_Null, x => x.IsFile(OTHER_FIELD_NAME).IsFileWithMessage(OTHER_MESSAGE));

        Assert.IsFalse(result.IsValid);

        var exception = Assert.ThrowsExactly<ValidationsException>(() => result.Throw());

        Assert.IsNotNull(exception);
        Assert.IsTrue(exception.Contains(Messages.File(value)));
        Assert.IsTrue(exception.Contains(Messages.File(value, FIELD_NAME)));
        Assert.IsTrue(exception.Contains(Messages.File()));
        Assert.IsTrue(exception.Contains(Messages.File(fieldName: FIELD_NAME)));
        Assert.IsTrue(exception.Contains(MESSAGE));
        Assert.IsTrue(exception.Contains(Messages.File(fieldName: OTHER_FIELD_NAME)));
        Assert.IsTrue(exception.Contains(OTHER_MESSAGE));
    }
}
