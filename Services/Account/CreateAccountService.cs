using CoreLib.Crypto;
using CoreLib.Helpers;
using CoreLib.Models.Dtos.Account.Create;
using CoreLib.Models.Entitys;
using CoreLib.Models.Entitys.Devices;
using CoreLib.Storage;

namespace CoreLib.Services.Account
{
    public class CreateAccountService(
        IDeviceInfoProvider deviceInfoProvider,
        IDeviceService deviceService,
        IPreKeyService preKeyService,
        IProofOfWorkService proofOfWork,
        IAccountStorage accountStorage,
        IDeviceStorage deviceStorage,
        IPreKeyStorage preKeyStorage,
        IShakeGenerator shakeGenerator,
        IApiService apiService)
    {
        public async Task<bool> CreateAsync(string nickName)
        {
            try
            {
                CoreLib.Models.Entitys.Account account = new()
                {
                    NickName = nickName
                };

                var (deviceData, SPrKSignatire, mLDsaPrK) = await deviceService.CreateAsync(await deviceInfoProvider.GetDeviceName());

                List<PreKey> preKeys = [];

                for (int i = 0; i < 50; i++)
                {
                    var preKey = await preKeyService.CreateAsync(mLDsaPrK, deviceData.Id);
                    preKeys.Add(preKey);
                }

                account.Id = await shakeGenerator.ToBase64StringAsync(await shakeGenerator.ComputeHash256Async(deviceData.DeviceKeys.SPK, 64));
                deviceData.AccountId = account.Id;
                var payload = await CreateRequestDTO(account, deviceData, preKeys, SPrKSignatire);

                var responce = await apiService.PostAsync(payload, mLDsaPrK, "account/register");

                if (responce.IsSuccessStatusCode)
                {
                    if (!await accountStorage.SaveAccountAsync(account))
                    {
                        throw new Exception("Error saving account");
                    }

                    if (!await deviceStorage.SaveDeviceAsync(deviceData))
                    {
                        throw new Exception("Error saving device");
                    }

                    if (!await preKeyStorage.AddRangeAsync(preKeys))
                    {
                        throw new Exception("Error saving prekeys");
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating account", ex);
            }
        }

        private async Task<CreateAccountRequest> CreateRequestDTO(CoreLib.Models.Entitys.Account account, Device device, List<PreKey> preKeys, byte[] sPrKSignatire)
        {
            var (nonce, proof) = await proofOfWork.FindProofOfWork(Convert.ToBase64String(device.DeviceKeys.SPK));
            CreateAccountRequest accountRequest = new()
            {
                Id = account.Id,
                Username = account.NickName,
                ProofOfWork = proof,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Nonce = nonce,
                ChaptchaToken = "ChaptchaToken",
                Device = new RegisterDeviceRequest
                {
                    Id = device.Id,
                    Name = device.DeviceName,
                    SPK = device.DeviceKeys.SPK,
                    Signature = sPrKSignatire,
                    PreKeys = [.. preKeys.Select(x => new RegisterPreKeyRequest
                    {
                        Id = x.Id,
                        PK = x.PK,
                        PKSignature = x.Signature
                    })]
                }
            };
            return accountRequest;
        }
    }
}
