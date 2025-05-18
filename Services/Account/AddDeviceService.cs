using CoreLib.Crypto;
using CoreLib.Helpers;
using CoreLib.Models.Dtos.Device.Add;
using CoreLib.Models.Entitys;
using CoreLib.Storage;
using CoreLib.Utils;
using MessagePack;
using System.Net.Http.Headers;

namespace CoreLib.Services.Account
{
    public interface IAddDeviceService
    {
        Task<bool> AddAsync(string deviceName, string aesKey);
        Task<AddDeviceQrCore> GetDeviceQrData();
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
                byte[] aesKeyBytes = Convert.FromBase64String(aesKey);
                if (aesKeyBytes.Length != 32)
                    throw new ArgumentException();

                var (device, mLDsaPrK) = await deviceService.CreateAsync(deviceName);

                List<PreKey> preKeys = [];

                var account = await accountStorage.GetAccountAsync();
                var currentDevice = await deviceStorage.GetCurrentDevice();
                for (int i = 0; i < 50; i++)
                {
                    var preKey = await preKeyService.CreateAsync(mLDsaPrK, device.Id);
                    preKeys.Add(preKey);
                }

                var payload = new NewDevicePayload
                {
                    Name = deviceName,
                    AccountId = account.Id,
                    DeviceId = device.Id,
                    SPK = device.SPK,
                    SPrK = device.SPrK,
                    SPKSignature = device.SPKSignature,
                    PreKeys = [.. preKeys.Select(pk => new NewDevicePreKey
                    {
                        Id = pk.Id,
                        SPK = pk.PK,
                        SPrK = pk.PrK,
                        Signature = pk.Signature
                    })]
                };

                byte[] rawNewDeviceRequest = await CreateDevicePayload(payload, aesKeyBytes);

                var currentSPrK = await mLDsaKey.RecoverPrivateKeyAsync(currentDevice.SPrK);

                NewDeviceRequest request = new();

                request.Payload = rawNewDeviceRequest;
                request.TempId = Convert.ToBase64String(await shakeGenerator.ComputeHash256(aesKeyBytes, 64));
                request.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                request.TrustedSignature = await aesGCMKey.EncryptAsync(await mLDsaKey.SignAsync(rawNewDeviceRequest, currentSPrK), aesKeyBytes);
                request.PayloadHash = await shakeGenerator.ComputeHash256(rawNewDeviceRequest, 64);
                request.TrustedDeviceId = currentDevice.Id;

                byte[] msgpackBytes = MessagePackSerializer.Serialize(request);

                byte[] requestSignature = await mLDsaKey.SignAsync(msgpackBytes, currentSPrK);

                var httpClient = new HttpClient();

                var httpContent = new ByteArrayContent(msgpackBytes);

                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-msgpack");

                httpContent.Headers.Add("X-Signature", Convert.ToBase64String(requestSignature));

                var response = await httpClient.PostAsync("https://localhost:7111/api/device/add", httpContent);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        public async Task<byte[]> CreateDevicePayload(NewDevicePayload rawPayload, byte[] aesKeyBytes)
        {
            byte[] rawBytes = MessagePackSerializer.Serialize(rawPayload);

            byte[] compressed = CompressionUtils.CompressGzip(rawBytes);

            byte[] encryptedPayload = await aesGCMKey.EncryptAsync(compressed, aesKeyBytes);

            return encryptedPayload;
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
