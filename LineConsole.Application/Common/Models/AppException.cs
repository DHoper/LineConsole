namespace LineConsole.Server.Models.Api;

/// <summary>
/// �i��a���~�N�X�P�T�������μh�ҥ~
/// </summary>
public class AppException : Exception
{
    public string Code { get; }

    public AppException(string code, string message) : base(message)
    {
        Code = code;
    }
}