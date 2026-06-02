namespace DC_bot_tests.TestHelperFiles;

public sealed class TestEnvironmentVariableScope : IDisposable
{
    private readonly Dictionary<string, string?> _originalValues = new();

    public TestEnvironmentVariableScope(IReadOnlyDictionary<string, string?> values)
    {
        foreach (var pair in values)
        {
            _originalValues[pair.Key] = Environment.GetEnvironmentVariable(pair.Key);
            Environment.SetEnvironmentVariable(pair.Key, pair.Value);
        }
    }

    public void Dispose()
    {
        foreach (var pair in _originalValues)
        {
            Environment.SetEnvironmentVariable(pair.Key, pair.Value);
        }
    }
}
