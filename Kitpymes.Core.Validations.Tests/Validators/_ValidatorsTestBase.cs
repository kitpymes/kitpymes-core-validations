namespace Kitpymes.Core.Validations.Tests.Validators;

public class ValidatorsTestBase
{
    protected const string FIELD_NAME = nameof(FIELD_NAME);
    protected const string MESSAGE = nameof(MESSAGE);
    protected const string OTHER_FIELD_NAME = nameof(OTHER_FIELD_NAME);
    protected const string OTHER_MESSAGE = nameof(OTHER_MESSAGE);

    private static bool _isInitialized = false;
    private readonly object _locker = new();

    public ValidatorsTestBase()
    {
        lock (_locker)
        {
            if (!_isInitialized)
            {
                TestClassInitialize();
                _isInitialized = true;
            }
        }
    }

    public void TestClassInitialize()
    {
        var culture = System.Globalization.CultureInfo.GetCultureInfo("es");
        System.Threading.Thread.CurrentThread.CurrentCulture = culture;
        System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
    }
}
