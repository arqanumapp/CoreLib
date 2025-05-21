using CoreLib.Crypto;
using CoreLib.Models.Dtos.Contact.Lookup;
using CoreLib.Models.Entitys.Devices;
using CoreLib.Models.ViewModels.Contact.Lookup;
using CoreLib.Storage;
using MessagePack;
using Org.BouncyCastle.Crypto.Parameters;

namespace CoreLib.Services.Account
{
    public interface IContactService
    {
        Task<FindContactResponce> FindAccountByIdAsync(string accountId);
    }
    internal class ContactService(IDeviceStorage deviceStorage, IMLDsaKey mLDsaKey, IApiService apiService) : IContactService
    {
        public async Task<FindContactResponce> FindAccountByIdAsync(string accountId)
        {
            Device currentDevice = await deviceStorage.GetCurrentDevice() ?? throw new Exception("Device not found");

            LookupUserRequest lookupUserRequest = new()
            {
                DeviceId = currentDevice.Id,
                AccountId = accountId,
            };

            MLDsaPrivateKeyParameters currentSPrK = await mLDsaKey.RecoverPrivateKeyAsync(currentDevice.DeviceKeys.SPK);

            var response = await apiService.PostAsync(lookupUserRequest, currentSPrK, "contact/lookup");

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

        public async Task SendHandshakeAsync(string accountId)
        {
            
        }
    }
}
