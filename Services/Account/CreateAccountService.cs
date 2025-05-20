using CoreLib.Crypto;
using CoreLib.Helpers;
using CoreLib.Models.Dtos.Account.Create;
using CoreLib.Models.Entitys;
using CoreLib.Storage;
using MessagePack;
using Org.BouncyCastle.Crypto.Parameters;
using System.Net.Http.Headers;

namespace CoreLib.Services.Account
{
    public class CreateAccountService(IDeviceInfoProvider deviceInfoProvider,
        IDeviceService deviceService,
        IPreKeyService preKeyService,
        IMLDsaKey mLDsaKey,
        IProofOfWorkService proofOfWork,
        IAccountStorage accountStorage,
        IDeviceStorage deviceStorage,
        IPreKeyStorage preKeyStorage)
    {
        public async Task<bool> CreateAsync(string nickName)
        {
            try
            {
                CoreLib.Models.Entitys.Account account = new()
                {
                    NickName = nickName
                };

                var (deviceData, mLDsaPrK) = await deviceService.CreateAsync(await deviceInfoProvider.GetDeviceName());

                List<PreKey> preKeys = [];

                for (int i = 0; i < 50; i++)
                {
                    var preKey = await preKeyService.CreateAsync(mLDsaPrK, deviceData.Id);
                    preKeys.Add(preKey);
                }

                var shakeGen = new ShakeGenerator();
                account.Id = await shakeGen.ToBase64StringAsync(await shakeGen.ComputeHash256Async(deviceData.SPK, 64));
                deviceData.AccountId = account.Id;
                var request = await CreateRequestDTO(account, deviceData, preKeys);

                var requestResult = await RegisterAccountAsync(request, mLDsaPrK);

                if (requestResult)
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
        private async Task<bool> RegisterAccountAsync(CreateAccountRequest request, MLDsaPrivateKeyParameters mLDsaPrivateKey)
        {

            byte[] msgpackBytes = MessagePackSerializer.Serialize(request);

            byte[] requestSignature = await mLDsaKey.SignAsync(msgpackBytes, mLDsaPrivateKey);

            var httpClient = new HttpClient();

            var httpContent = new ByteArrayContent(msgpackBytes);

            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-msgpack");

            httpContent.Headers.Add("X-Signature", Convert.ToBase64String(requestSignature));

            var response = await httpClient.PostAsync("https://localhost:7111/api/account/add", httpContent);

            return response.IsSuccessStatusCode;
        }



        private async Task<CreateAccountRequest> CreateRequestDTO(CoreLib.Models.Entitys.Account account, Device device, List<PreKey> preKeys)
        {
            var (nonce, proof) = await proofOfWork.FindProofOfWork(Convert.ToBase64String(device.SPK));
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
                    SPK = device.SPK,
                    Signature = device.SPKSignature,
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
