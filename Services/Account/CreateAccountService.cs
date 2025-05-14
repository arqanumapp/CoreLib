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
    public class CreateAccountService(IDeviceInfoProvider deviceInfoProvider)
    {
        public async Task<bool> CreateAsync(string nickName)
        {
            try
            {
                CoreLib.Models.Entitys.Account account = new()
                {
                    NickName = nickName
                };

                var deviceService = new DeviceService();

                var (deviceData, DilitiumPrK) = await deviceService.CreateAsync("test");

                var preKeyService = new PreKeyService();

                List<PreKey> preKeys = [];

                for (int i = 0; i < 50; i++)
                {
                    var preKey = await preKeyService.CreateAsync(DilitiumPrK, deviceData.Id);
                    preKeys.Add(preKey);
                }

                var shakeGen = new ShakeGenerator();
                account.Id = await shakeGen.GetString(await shakeGen.ComputeHash256(deviceData.SPK, 64));

                var request = await CreateRequestDTO(account, deviceData, preKeys);

                var requestResult = await RegisterAccountAsync(request, DilitiumPrK);

                if (requestResult)
                {
                    var accountStorage = new AccountStorage();

                    if (!await accountStorage.SaveAccountAsync(account))
                    {
                        throw new Exception("Error saving account");
                    }

                    var deviceStorage = new DeviceStorage();

                    if (!await deviceStorage.SaveDeviceAsync(deviceData))
                    {
                        throw new Exception("Error saving device");
                    }

                    var preKeyStorage = new PreKeyStorage();

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

            var dilithiumKey = new DilitiumKey();
            byte[] signature = await dilithiumKey.SignAsync(rawBytes, mLDsaPrivateKey);

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

            Console.WriteLine(BitConverter.ToString([.. compressedBytes.Take(64)]));

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
            var proofOfWork = new ProofOfWorkService();
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
                    SPK = Convert.ToBase64String(device.SPK),
                    Signature = Convert.ToBase64String(device.SPKSignature),
                    PreKeys = [.. preKeys.Select(x => new RegisterPreKeyRequest
                    {
                        Id = x.Id,
                        PK = Convert.ToBase64String(x.PK),
                        PKSignature = Convert.ToBase64String(x.Signature)
                    })]
                }
            };
            return accountRequest;
        }
    }
}
