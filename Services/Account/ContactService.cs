using CoreLib.Crypto;
using CoreLib.Models.Dtos.Contact.Lookup;
using CoreLib.Models.Entitys;
using CoreLib.Models.ViewModels.Contact.Lookup;
using CoreLib.Storage;
using MessagePack;
using Org.BouncyCastle.Crypto.Parameters;
using System.Net.Http.Headers;

namespace CoreLib.Services.Account
{
    public interface IContactService
    {
        Task<FindContactResponce> FindAccountById(string accountId);
    }
    internal class ContactService(IDeviceStorage deviceStorage, IMLDsaKey mLDsaKey) : IContactService
    {
        public async Task<FindContactResponce> FindAccountById(string accountId)
        {
            Device currentDevice = await deviceStorage.GetCurrentDevice() ?? throw new Exception("Device not found");

            LookupUserRequest lookupUserRequest = new()
            {
                DeviceId = currentDevice.Id,
                AccountId = accountId,
            };

            byte[] msgpackBytes = MessagePackSerializer.Serialize(lookupUserRequest);

            MLDsaPrivateKeyParameters currentSPrK = await mLDsaKey.RecoverPrivateKeyAsync(currentDevice.SPrK);

            byte[] requestSignature = await mLDsaKey.SignAsync(msgpackBytes, currentSPrK);

            var httpClient = new HttpClient();

            var httpContent = new ByteArrayContent(msgpackBytes);

            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-msgpack");

            httpContent.Headers.Add("X-Signature", Convert.ToBase64String(requestSignature));

            var response = await httpClient.PostAsync("https://localhost:7111/api/contact/lookup", httpContent);

            if (response.IsSuccessStatusCode)
            {
                var responseBytes = await response.Content.ReadAsByteArrayAsync();
                var result = MessagePackSerializer.Deserialize<LookupUserResponse>(responseBytes);
                FindContactResponce contactResponce = new()
                {
                    Nick = result.Nick,
                    AccountId = accountId,
                };
                return contactResponce;
            }
            else return null;
        }
    }
}
