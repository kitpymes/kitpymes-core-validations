using Kitpymes.Core.Shared.Util;
using Kitpymes.Core.Validations.Abstractions.Exceptions;
using Kitpymes.Core.Validations.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kitpymes.Core.Validations.Tests.Validators;

[TestClass]
public class MultiplesRulesTests : ValidatorsTestBase
{
    [TestMethod]
    public void Example_Rules()
    {
        var min = FakeTypes.ValueTypes.SimpleTypes.Int_Max;
        var max = FakeTypes.ValueTypes.SimpleTypes.Int_Min;

        var result = Validator
            .AddRule(FakeTypes.ValueTypes.SimpleTypes.Int_Null, x => x
                .IsNullOrEmpty())
            .AddRule(FakeTypes.ValueTypes.SimpleTypes.Decimal_Null, x => x
                .IsNullOrEmpty(FIELD_NAME))
            .AddRule(FakeTypes.ValueTypes.SimpleTypes.Int_Null, x => x
                .IsNullOrEmptyWithMessage(MESSAGE))
            .AddRule(FakeTypes.ReferenceTypes.ClassTypes.String_Default, x => x
                .IsDirectory()
                .IsEmail()
                .IsFileExtension()
                .IsFile()
                .IsName()
                .IsSubdomain())
            .AddRule(FakeTypes.ReferenceTypes.ClassTypes.String_Null, x => x
                .IsDirectory()
                .IsEmail()
                .IsFileExtension()
                .IsFile()
                .IsName()
                .IsSubdomain())
            .AddRule(FakeTypes.ReferenceTypes.ClassTypes.String_New(), x => x
                .IsDirectory(FIELD_NAME)
                .IsEmail(FIELD_NAME)
                .IsFileExtension(FIELD_NAME)
                .IsFile(FIELD_NAME)
                .IsName(FIELD_NAME)
                .IsSubdomain(FIELD_NAME))
            .AddRule(FakeTypes.ReferenceTypes.ClassTypes.String_Empty, x => x
                .IsDirectoryWithMessage(MESSAGE)
                .IsEmailWithMessage(MESSAGE)
                .IsFileExtensionWithMessage(MESSAGE)
                .IsFileWithMessage(OTHER_MESSAGE)
                .IsNameWithMessage(OTHER_MESSAGE)
                .IsSubdomainWithMessage(OTHER_MESSAGE));

        Assert.IsFalse(result.IsValid);

        var exception = Assert.ThrowsExactly<ValidationsException>(() => result.Throw());

        Assert.IsNotNull(exception);
        Assert.IsTrue(exception.Contains(Messages.NullOrEmpty()));
        Assert.IsTrue(exception.Contains(Messages.NullOrEmpty(FIELD_NAME)));  

        Assert.IsTrue(exception.Contains(Messages.Directory(FakeTypes.ReferenceTypes.ClassTypes.String_Default)));
        Assert.IsTrue(exception.Contains(Messages.Email(FakeTypes.ReferenceTypes.ClassTypes.String_Default)));
        Assert.IsTrue(exception.Contains(Messages.FileExtension(FakeTypes.ReferenceTypes.ClassTypes.String_Default)));
        Assert.IsTrue(exception.Contains(Messages.File(FakeTypes.ReferenceTypes.ClassTypes.String_Default)));
        Assert.IsTrue(exception.Contains(Messages.Name(FakeTypes.ReferenceTypes.ClassTypes.String_Default)));
        Assert.IsTrue(exception.Contains(Messages.Subdomain(FakeTypes.ReferenceTypes.ClassTypes.String_Default)));

        Assert.IsTrue(exception.Contains(Messages.Directory(FakeTypes.ReferenceTypes.ClassTypes.String_New(), FIELD_NAME)));
        Assert.IsTrue(exception.Contains(Messages.Email(FakeTypes.ReferenceTypes.ClassTypes.String_New(), FIELD_NAME)));
        Assert.IsTrue(exception.Contains(Messages.FileExtension(FakeTypes.ReferenceTypes.ClassTypes.String_New(), FIELD_NAME)));
        Assert.IsTrue(exception.Contains(Messages.File(FakeTypes.ReferenceTypes.ClassTypes.String_New(), FIELD_NAME)));
        Assert.IsTrue(exception.Contains(Messages.Name(FakeTypes.ReferenceTypes.ClassTypes.String_New(), FIELD_NAME)));
        Assert.IsTrue(exception.Contains(Messages.Subdomain(FakeTypes.ReferenceTypes.ClassTypes.String_New(), FIELD_NAME)));

        Assert.IsTrue(exception.Contains(MESSAGE));
        Assert.IsTrue(exception.Contains(OTHER_MESSAGE));
    }

    [TestMethod]
    public void Example_Rules_StopFirstError()
    {
        var min = FakeTypes.ValueTypes.SimpleTypes.Int_Max;
        var max = FakeTypes.ValueTypes.SimpleTypes.Int_Min;

        var result = Validator
            .AddRule(FakeTypes.ValueTypes.SimpleTypes.Int_Null, x => x
                .IsNullOrEmpty())
            .AddRule(FakeTypes.ValueTypes.SimpleTypes.Decimal_Null, x => x
                .IsNullOrEmpty())
            .AddRule(FakeTypes.ValueTypes.SimpleTypes.Decimal_Null, x => x
                .IsNullOrEmpty(FIELD_NAME))
            .AddRule(FakeTypes.ValueTypes.SimpleTypes.Int_Null, x => x
                .IsNullOrEmptyWithMessage(MESSAGE))
            .StopFirstError();

        Assert.IsFalse(result.IsValid);

        var exception = Assert.ThrowsExactly<ValidationsException>(() => result.Throw());

        Assert.IsNotNull(exception);
        Assert.IsTrue(exception.Contains(Messages.NullOrEmpty()));
    }
}
