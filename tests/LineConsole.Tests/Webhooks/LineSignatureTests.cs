using System.Text;
using LineConsole.Application.Webhooks;

namespace LineConsole.Tests.Webhooks;

/// <summary>
/// X-Line-Signature = Base64(HMAC-SHA256(Channel Secret, 原始 body))。
/// 參考值是用 openssl 另外算的，不是拿被測的程式自己算自己比：
/// printf '%s' '&lt;body&gt;' | openssl dgst -sha256 -hmac 'test-channel-secret' -binary | base64
/// </summary>
public class LineSignatureTests
{
    private const string Secret = "test-channel-secret";
    private const string Body = """{"destination":"Uxxx","events":[{"type":"follow","webhookEventId":"01M89PGCWM33QNX40THP8FFSCE"}]}""";
    private const string OpensslSignature = "dRNDJ8JBl8TXnzvit7T8qtdiddxV7awnVYnsQ07Hi9U=";

    private static byte[] Bytes(string s) => Encoding.UTF8.GetBytes(s);

    [Fact]
    public void Compute_matches_openssl() =>
        Assert.Equal(OpensslSignature, LineSignature.Compute(Secret, Bytes(Body)));

    [Fact]
    public void Correct_signature_is_valid() =>
        Assert.True(LineSignature.IsValid(Secret, Bytes(Body), OpensslSignature));

    [Fact]
    public void Body_changed_by_one_character_is_invalid() =>
        Assert.False(LineSignature.IsValid(Secret, Bytes(Body.Replace("follow", "Follow")), OpensslSignature));

    [Fact]
    public void Signed_with_another_secret_is_invalid() =>
        Assert.False(LineSignature.IsValid("another-secret", Bytes(Body), OpensslSignature));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not base64 !!")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAA==")]                       // 16 bytes：長度不對
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=")]   // 32 bytes 但全是 0
    public void Missing_or_malformed_signature_is_invalid(string? signature) =>
        Assert.False(LineSignature.IsValid(Secret, Bytes(Body), signature));

    [Fact]
    public void Empty_secret_never_validates() =>
        Assert.False(LineSignature.IsValid("", Bytes(Body), LineSignature.Compute("", Bytes(Body))));
}
