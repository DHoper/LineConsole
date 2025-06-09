using System.Net.Http.Headers;

namespace LineConsole.Infrastructure.Http;

/// <summary>
/// �ʸ˹�~�� API �� HTTP �I�s�\��A�䴩 GET�BPOST�BPUT�BDELETE ���q�ξާ@
/// </summary>
public interface IExternalHttpClient
{
    /// <summary>
    /// �o�e GET �ШD�A�^�ǫ��w���O���G
    /// </summary>
    Task<T?> GetAsync<T>(string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// �o�e POST �ШD�A���a JSON ���
    /// </summary>
    Task<T?> PostAsync<T>(string url, object data, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// �o�e POST �ШD�A���a byte stream�]�G�i��W�ǥγ~�^
    /// </summary>
    Task PostStreamAsync(string url, Stream stream, string contentType, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// �o�e GET �ШD�A���o��l�G�i���y�]�Ҧp�U���Ϥ��^
    /// </summary>
    Task<Stream> GetStreamAsync(string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// �o�e DELETE �ШD
    /// </summary>
    Task DeleteAsync(string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// �o�e POST �ШD�����w������^�Ǹ�ơ]�Ҧp�]�w���\�^
    /// </summary>
    Task PostAsync(string url, object data, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
}
