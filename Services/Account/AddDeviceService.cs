using CoreLib.Crypto;
using CoreLib.Helpers;
using CoreLib.Models.Dtos.Device.Add;
using CoreLib.Models.Entitys;
using CoreLib.Storage;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Text;
using System.IO.Compression;

namespace CoreLib.Services.Account
{
    public interface IAddDeviceService
    {
        Task<bool> AddAsync(string deviceName, string aesKey);
    }
    internal class AddDeviceService(
        IDeviceService deviceService,
        IAesGCMKey aesGCMKey,
        IDeviceInfoProvider deviceInfoProvider,
        IPreKeyService preKeyService,
        IAccountStorage accountStorage,
        IDeviceStorage deviceStorage,
        IShakeGenerator shakeGenerator,
        IMLDsaKey mLDsaKey) : IAddDeviceService
    {
        public async Task<bool> AddAsync(string deviceName, string aesKey)
        {
            try
            {
                if (Convert.FromBase64String(aesKey).Length != 32)
                    throw new ArgumentException("AES key must be 256-bit (32 bytes)");

                var (device, mLDsaPrK) = await deviceService.CreateAsync(deviceName);

                List<PreKey> preKeys = [];

                var account = await accountStorage.GetAccountAsync() ?? throw new ArgumentNullException();
                var currentDevice = await deviceStorage.GetCurrentDevice() ?? throw new ArgumentNullException();
                for (int i = 0; i < 50; i++)
                {
                    var preKey = await preKeyService.CreateAsync(mLDsaPrK, device.Id);
                    preKeys.Add(preKey);
                }
                var payload = new NewDevicePayload
                {
                    Name = deviceName,
                    AccountId = account.Id,
                    Id = device.Id,
                    SPK = Convert.ToBase64String(device.SPK),
                    SPrK = Convert.ToBase64String(device.SPrK),
                    SPKSignature = Convert.ToBase64String(device.SPKSignature),
                    PreKeys = [.. preKeys.Select(pk => new NewDevicePreKey
                    {
                        Id = pk.Id,
                        SPK = Convert.ToBase64String(pk.PK),
                        SPrK = Convert.ToBase64String(pk.PrK),
                        Signature = Convert.ToBase64String(pk.Signature)
                    })]
                };

                var settings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    Formatting = Formatting.None
                };

                string rawPayloadJson = JsonConvert.SerializeObject(payload, settings);

                byte[] rawPayloadBytes = Encoding.UTF8.GetBytes(rawPayloadJson);

                var currentSPrK = await mLDsaKey.RecoverPrivateKeyAsync(currentDevice.SPrK);

                byte[] rawNewDeviceIDSignature = await mLDsaKey.SignAsync(Encoding.UTF8.GetBytes(payload.Id), currentSPrK);

                byte[] aesKeyBytes = Convert.FromBase64String(aesKey);

                NewDeviceRequest request = new()
                {
                    Payload = Convert.ToBase64String(await aesGCMKey.EncryptAsync(rawPayloadBytes, aesKeyBytes)),
                    TrustedDeviceId = currentDevice.Id,
                    TempId = Convert.ToBase64String(await shakeGenerator.ComputeHash256(aesKeyBytes, 64)),
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    TrustedSignature = Convert.ToBase64String(await aesGCMKey.EncryptAsync(rawNewDeviceIDSignature, aesKeyBytes)),
                    PayloadHash = Convert.ToBase64String(await shakeGenerator.ComputeHash256(rawPayloadBytes, 64)),
                };
                string rawRequestJson = JsonConvert.SerializeObject(request, settings);
                byte[] rawRequestBytes = Encoding.UTF8.GetBytes(rawRequestJson);
                byte[] rawRequestJsonSignature = await mLDsaKey.SignAsync(rawRequestBytes, currentSPrK);
                byte[] compressedBytes;
                using (var uncompressedStream = new MemoryStream(rawRequestBytes))
                using (var compressedStream = new MemoryStream())
                {
                    using (var gzipStream = new GZipStream(compressedStream, CompressionLevel.Optimal, leaveOpen: true))
                    {
                        await uncompressedStream.CopyToAsync(gzipStream);
                    }

                    compressedBytes = compressedStream.ToArray();
                }
                var httpClient = new HttpClient();
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7111/api/device/add")
                {
                    Content = new ByteArrayContent(compressedBytes)
                };
                httpRequest.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                httpRequest.Content.Headers.ContentEncoding.Add("gzip");

                httpRequest.Headers.Add("X-Signature", Convert.ToBase64String(rawRequestJsonSignature));
                var response = await httpClient.SendAsync(httpRequest);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        public async Task<AddDeviceQrCore> GetDeviceQrData()
        {
            var aseKey = await aesGCMKey.GenerateKey();
            var deviceName = await deviceInfoProvider.GetDeviceName();
            return new AddDeviceQrCore
            {
                Name = deviceName,
                Key = Convert.ToBase64String(aseKey)
            };
        }
    }
}
