using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LineConsole.Application.Infrastructure.Interfaces
{
    /// <summary>
    /// �ʸ˻P LINE Messaging API ���q���̧C�h�ųq�� HTTP �ШD�]�䴩�h�b�� token�^
    /// </summary>
    public interface ILineClient
    {
        /// <summary>
        /// �ǰe JSON �ШD�øѪR�����w���O���G
        /// </summary>
        /// <typeparam name="T">�w�����^�Ǹ�ƫ��O</typeparam>
        /// <param name="endpoint">LINE API ���۹���|</param>
        /// <param name="method">HTTP ��k�]POST �� GET�^</param>
        /// <param name="accessToken">��e�Τ᪺ LINE Channel Access Token</param>
        /// <param name="data">�ǰe����ƪ���</param>
        /// <param name="useDataDomain">�O�_������ api-data.line.me�]�W�ǹϤ��ݳ]�w�� true�^</param>
        /// <param name="ct">���������v��</param>
        Task<T?> SendJsonAsync<T>(
            string endpoint,
            HttpMethod method,
            string accessToken,
            object? data = null,
            bool useDataDomain = false,
            CancellationToken ct = default
        );

        /// <summary>
        /// �ǰe JSON �ШD�A�����ݦ^�Ǹ�ơ]�Ҧp validate�Bset default ���^
        /// </summary>
        Task SendJsonAsync(
            string endpoint,
            HttpMethod method,
            string accessToken,
            object? data = null,
            bool useDataDomain = false,
            CancellationToken ct = default
        );

        /// <summary>
        /// �ǰe��y���e�]�W�ǹϤ��� Rich Menu �Ρ^
        /// </summary>
        Task SendStreamAsync(
            string endpoint,
            Stream stream,
            string contentType,
            string accessToken,
            CancellationToken ct = default
        );

        /// <summary>
        /// �U���Ϥ���y�]���o Rich Menu �Ϥ��Ρ^
        /// </summary>
        Task<Stream> GetStreamAsync(
            string endpoint,
            string accessToken,
            CancellationToken ct = default
        );

        /// <summary>
        /// �o�X GET �ШD�øѪR JSON ���G�����w���O
        /// </summary>
        /// <typeparam name="T">�w�����^�Ǹ�ƫ��O</typeparam>
        Task<T?> GetJsonAsync<T>(
            string endpoint,
            string accessToken,
            bool useDataDomain = false,
            CancellationToken ct = default
        );

        /// <summary>
        /// �o�X DELETE �ШD�A�����ݦ^�Ǥ��e
        /// </summary>
        Task DeleteAsync(
            string endpoint,
            string accessToken,
            CancellationToken ct = default
        );
    }
}
