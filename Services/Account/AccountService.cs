using CoreLib.Crypto;
using CoreLib.Interfaces;
using CoreLib.Models.Dtos.Account.Create;
using CoreLib.Models.Entitys;
using CoreLib.Models.ViewModels.Account;
using CoreLib.Storage;

namespace CoreLib.Services.Account
{
    public class AccountService(
        IDeviceInfoProvider deviceInfoProvider,
        IDeviceService deviceService,
        IPreKeyService preKeyService,
        IProofOfWorkService proofOfWork,
        IAccountStorage accountStorage,
        IDeviceStorage deviceStorage,
        IPreKeyStorage preKeyStorage,
        IShakeGenerator shakeGenerator,
        IApiService apiService,
        ICaptchaTokenProvider captchaTokenProvider)
    {
        public async Task<bool> CreateAsync(string nickName, IProgress<string>? progress = null)
        {
            try
            {
                CoreLib.Models.Entitys.Account account = new()
                {
                    NickName = nickName
                };
                progress?.Report("Generating device keys...");
                var (deviceData, SPrKSignatire, mLDsaPrK) = await deviceService.CreateAsync(await deviceInfoProvider.GetDeviceName());

                List<PreKey> preKeys = [];
                for (int i = 0; i < 50; i++)
                {
                    var preKey = await preKeyService.CreateAsync(mLDsaPrK, deviceData.Id);
                    preKeys.Add(preKey);
                }
                account.Id = await shakeGenerator.ToBase64StringAsync(await shakeGenerator.ComputeHash256Async(deviceData.DeviceKeys.SPK, 64));
                deviceData.AccountId = account.Id;

                var proofOfWorkProgress = new Progress<string>(message =>
                {
                    progress?.Report(message);
                });

                var (nonce, proof) = await proofOfWork.FindProofOfWork(Convert.ToBase64String(deviceData.DeviceKeys.SPK), proofOfWorkProgress);

                var payload = await CreateRequestDTO(account, deviceData, preKeys, SPrKSignatire, proof, nonce);

                progress?.Report("Registration...");

                var responce = await apiService.PostAsync(payload, mLDsaPrK, "account/register");

                if (responce.IsSuccessStatusCode)
                {
                    progress?.Report("Registration successful!");
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
                return responce.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                progress?.Report("Error creating account: " + ex.Message);
                return false;
            }
        }
        public async Task<AccountInfoViewModel> GetAccountInfoAsync()
        {
            var account = await accountStorage.GetAccountAsync() ?? throw new ArgumentNullException("Account not found");
            var model = new AccountInfoViewModel
            {
                NickName = account.NickName,
                AccountId = account.Id
            };
            return model;
        }
        private async Task<CreateAccountRequest> CreateRequestDTO(CoreLib.Models.Entitys.Account account, Models.Entitys.Devices.Device device, List<PreKey> preKeys, byte[] sPrKSignatire, string proof, string nonce)
        {
            CreateAccountRequest accountRequest = new()
            {
                Id = account.Id,
                Username = account.NickName,
                ProofOfWork = proof,
                Nonce = nonce,
                ChaptchaToken = await captchaTokenProvider.GetCaptchaTokenAsync() ?? throw new ArgumentNullException("hCaptcha token missing."),
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
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
