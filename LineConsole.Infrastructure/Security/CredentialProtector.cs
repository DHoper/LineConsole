using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;

namespace LineConsole.Infrastructure.Security;

/// <summary>
/// 加解密 LINE 官方帳號的憑證（Channel Secret、Access Token），用 ASP.NET Data Protection。
/// 金鑰存在資料庫（見 ApplicationDbContext），會自動輪替；舊金鑰保留，舊資料照樣解得開。
/// </summary>
public sealed class CredentialProtector
{
    private const string Purpose = "LineConsole.LineChannelCredentials.v1";
    private readonly IDataProtector _protector;
    private readonly ILogger<CredentialProtector> _logger;

    public CredentialProtector(IDataProtectionProvider provider, ILogger<CredentialProtector> logger)
    {
        _protector = provider.CreateProtector(Purpose);
        _logger = logger;
    }

    public string Protect(string plaintext) => _protector.Protect(plaintext);

    /// <summary>
    /// 解密。解不開的當成 M0 以前存的明碼舊資料，原樣回傳（下次寫入就會加密），並記一筆警告。
    /// </summary>
    public string Unprotect(string stored)
    {
        if (string.IsNullOrEmpty(stored))
            return stored;
        try
        {
            return _protector.Unprotect(stored);
        }
        catch (CryptographicException)
        {
            _logger.LogWarning("官方帳號憑證不是加密格式（可能是舊資料），先以明碼使用");
            return stored;
        }
    }
}
