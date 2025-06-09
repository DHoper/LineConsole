namespace LineConsole.Server.Models.Api;

/// <summary>�з� Web API �^�Ǯ榡�A�ʸ˷~�ȥN�X�B�T���P��Ƥ��e</summary>
public record ApiResponse<T>
{
    public required string Code { get; init; } // �~���޿�N�X�A�Ҧp SUCCESS�BVALIDATION_ERROR�BLINE_API_FAIL
    public required string Message { get; init; } // �ϥΪ̴��ܰT���A�i�䴩�h�y�t
    public T? Data { get; init; } // ��ڦ^�Ǫ���ơ]�i�ର null �ΪŪ���^

    /// <summary>�ާ@���\�A�^�Ǹ��</summary>
    public static ApiResponse<T> Success(T data, string message = "�ާ@���\") =>
        new() { Code = "SUCCESS", Message = message, Data = data };

    /// <summary>�ާ@���ѡA�^�ǿ��~�T���P���~�N�X</summary>
    public static ApiResponse<T> Fail(string code, string message) =>
        new() { Code = code, Message = message, Data = default };
}
