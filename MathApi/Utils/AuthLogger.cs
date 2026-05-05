namespace MathApis.Utils;

public sealed class AuthLogger
{
    private static AuthLogger _instance = new AuthLogger();
    
    private readonly StreamWriter _fileWriter;
    
    static AuthLogger() {}

    private AuthLogger()
    {
        _fileWriter = new StreamWriter("auth_errors.log", true);
        _fileWriter.AutoFlush = true;
    }

    public static AuthLogger Instance => _instance;

    public void LogError(string error)
    {
        string message = $"{DateTime.Now} - Error: {error}";
        _fileWriter.WriteLine(message);
    }
}