namespace LineConsole.Application.Infrastructure.Interfaces;

/// <summary>
/// 提供密碼雜湊與驗證的介面
/// </summary>
public interface IPasswordHasher
{
    /// <summary>將明文密碼轉換為雜湊值</summary>
    string Hash(string password);

    /// <summary>驗證使用者輸入的密碼是否符合雜湊值</summary>
    bool Verify(string hashedPassword, string providedPassword);
}
