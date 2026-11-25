using System.Security.Cryptography;
using System.Text;

namespace LineConsole.Application.Webhooks;

/// <summary>
/// LINE Webhook 的簽章：X-Line-Signature＝Base64(HMAC-SHA256(Channel Secret, 原始的請求內容))。
/// 一定要用「原始的位元組」算，不能先反序列化再序列化（空白、欄位順序一變，簽章就對不上）。
/// </summary>
public static class LineSignature
{
    public static string Compute(string channelSecret, byte[] body) =>
        Convert.ToBase64String(HMACSHA256.HashData(Encoding.UTF8.GetBytes(channelSecret), body));

    /// <summary>用固定時間比對，避免從回應時間猜出簽章（timing attack）</summary>
    public static bool IsValid(string channelSecret, byte[] body, string? signature)
    {
        if (string.IsNullOrWhiteSpace(signature) || string.IsNullOrEmpty(channelSecret))
            return false;

        Span<byte> provided = stackalloc byte[64];
        if (!Convert.TryFromBase64String(signature.Trim(), provided, out var written) || written != 32)
            return false;

        var expected = HMACSHA256.HashData(Encoding.UTF8.GetBytes(channelSecret), body);
        return CryptographicOperations.FixedTimeEquals(expected, provided[..written]);
    }
}
