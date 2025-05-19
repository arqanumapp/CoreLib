using CoreLib.Crypto;
using CoreLib.Helpers;
using CoreLib.Models.Dtos.Account.Create;
using CoreLib.Models.Entitys;
using CoreLib.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Org.BouncyCastle.Crypto.Parameters;
using System.IO.Compression;
using System.Text;

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
                account.Id = await shakeGen.GetString(await shakeGen.ComputeHash256(deviceData.SPK, 64));
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
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.None
            };

            string rawJson = JsonConvert.SerializeObject(request, settings);
            byte[] rawBytes = Encoding.UTF8.GetBytes(rawJson);

            byte[] signature = await mLDsaKey.SignAsync(rawBytes, mLDsaPrivateKey);

            byte[] compressedBytes;
            using (var uncompressedStream = new MemoryStream(rawBytes))
            using (var compressedStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(compressedStream, CompressionLevel.Optimal, leaveOpen: true))
                {
                    await uncompressedStream.CopyToAsync(gzipStream);
                }

                compressedBytes = compressedStream.ToArray();
            }

            var httpClient = new HttpClient();
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7111/api/account/register")
            {
                Content = new ByteArrayContent(compressedBytes)
            };

            httpRequest.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            httpRequest.Content.Headers.ContentEncoding.Add("gzip");

            httpRequest.Headers.Add("X-Signature", Convert.ToBase64String(signature));

            var response = await httpClient.SendAsync(httpRequest);
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
