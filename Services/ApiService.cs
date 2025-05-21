using CoreLib.Configurations;
using CoreLib.Crypto;
using MessagePack;
using Org.BouncyCastle.Crypto.Parameters;

namespace CoreLib.Services
{
    public interface IApiService
    {
        Task<HttpResponseMessage> PostAsync<TPayload>(TPayload payload, MLDsaPrivateKeyParameters sPrK, string route);
    }

    internal class ApiService(HttpClient httpClient, IMLDsaKey mLDsaKey, ApiConfiguration apiConfig) : IApiService
    {
        public async Task<HttpResponseMessage> PostAsync<TPayload>(TPayload payload, MLDsaPrivateKeyParameters sPrK, string route)
        {
            byte[] msgpackBytes = MessagePackSerializer.Serialize(payload);
            var httpContent = new ByteArrayContent(msgpackBytes);
            var signature = await mLDsaKey.SignAsync(msgpackBytes, sPrK);
            httpContent.Headers.Add("X-Signature", Convert.ToBase64String(signature));
            return await httpClient.PostAsync($"{apiConfig.ApiBaseUrl + route}", httpContent);
        }
    }
}
