using CoreLib.Crypto;
using CoreLib.Helpers;
using CoreLib.Models.Dtos.Device.Add;
using CoreLib.Models.Entitys;
using CoreLib.Storage;
using MessagePack;
using System.Net.Http.Headers;

namespace CoreLib.Services.Account
{
    public interface IAddDeviceService
    {
        Task<bool> StartDeviceProvisioningAsync(string deviceName, byte[] aesKeyBytes);
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
        public async Task<bool> StartDeviceProvisioningAsync(string deviceName, byte[] aesKeyBytes)
        {
            try
            {
                var (device, mLDsaPrK) = await deviceService.CreateAsync(deviceName);

                var account = await accountStorage.GetAccountAsync();
                var currentDevice = await deviceStorage.GetCurrentDevice();


                var privatePayload = new NewDevicePrivatePayloadRequest
                {
                    Name = deviceName,
                    AccountId = account.Id,
                    DeviceId = device.Id,
                    SPK = device.SPK,
                    SPrK = device.SPrK,
                    SPKSignature = device.SPKSignature,
                };

                var publicPayload = new NewDevicePublicPayloadRequest
                {
                    Name = deviceName,
                    AccountId = account.Id,
                    DeviceId = device.Id,
                    SPK = device.SPK,
                    SPKSignature = device.SPKSignature,
                };

                byte[] rawEncryptedPrivatePayload = await aesGCMKey.EncryptAsync(MessagePackSerializer.Serialize(privatePayload), aesKeyBytes);

                byte[] rawPublicPayload = MessagePackSerializer.Serialize(publicPayload);
                byte[] rawEncryptedPublicPayload = await aesGCMKey.EncryptAsync(rawPublicPayload, aesKeyBytes);

                var currentSPrK = await mLDsaKey.RecoverPrivateKeyAsync(currentDevice.SPrK);

                NewDeviceRequest request = new();

                request.PrivatePayload = rawEncryptedPrivatePayload;
                request.PublicPayload = rawEncryptedPublicPayload;
                request.TempId = Convert.ToBase64String(await shakeGenerator.ComputeHash256(aesKeyBytes, 64));
                request.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                request.TrustedSignature = await aesGCMKey.EncryptAsync(await mLDsaKey.SignAsync(rawPublicPayload, currentSPrK), aesKeyBytes);
                request.PrivatePayloadHash = await shakeGenerator.ComputeHash256(rawEncryptedPrivatePayload, 64);
                request.PublicPayloadHash = await shakeGenerator.ComputeHash256(rawEncryptedPublicPayload, 64);
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
        public async Task<bool> CompleteDeviceProvisioningAsync(byte[] responseData, byte[] aesKeyBytes)
        {
            try
            {
                var provisioningResponse = MessagePackSerializer.Deserialize<NewDeviceResponce>(responseData);

                var computedPublicHash = await shakeGenerator.ComputeHash256(provisioningResponse.PublicPayload, 64);
                var computedPrivateHash = await shakeGenerator.ComputeHash256(provisioningResponse.PrivatePayload, 64);

                if (!computedPublicHash.SequenceEqual(provisioningResponse.PublicPayloadHash) ||
                    !computedPrivateHash.SequenceEqual(provisioningResponse.PrivatePayloadHash))
                {
                    return false;
                }
                var publicPayloadBytes = await aesGCMKey.DecryptAsync(provisioningResponse.PublicPayload, aesKeyBytes);
                var privatePayloadBytes = await aesGCMKey.DecryptAsync(provisioningResponse.PrivatePayload, aesKeyBytes);
                var trustedSignatureBytes = await aesGCMKey.DecryptAsync(provisioningResponse.TrustedSignature, aesKeyBytes);

                var publicPayload = MessagePackSerializer.Deserialize<NewDevicePublicPayloadRequest>(publicPayloadBytes);
                var privatePayload = MessagePackSerializer.Deserialize<NewDevicePrivatePayloadRequest>(privatePayloadBytes);

                var preKeys = new List<PreKey>();
                var deviceSPrK = await mLDsaKey.RecoverPrivateKeyAsync(privatePayload.SPrK);

                for (int i = 0; i < 50; i++)
                {
                    var preKey = await preKeyService.CreateAsync(deviceSPrK, privatePayload.DeviceId);
                    preKeys.Add(preKey);
                }
                var request = new AddNewDeviceRequest();

                request.DevicePayload = publicPayloadBytes;

                request.PreKeysPayload = [.. preKeys.Select(x => new AddNewDevicePreKeysRequest
                    {
                        Id = x.Id,
                        PK = x.PK,
                        PKSignature = x.Signature
                    })];

                request.DeviceTrustedSignature = trustedSignatureBytes;

                byte[] msgpackBytes = MessagePackSerializer.Serialize(request);

                byte[] requestSignature = await mLDsaKey.SignAsync(msgpackBytes, deviceSPrK);

                var httpClient = new HttpClient();

                var httpContent = new ByteArrayContent(msgpackBytes);

                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-msgpack");

                httpContent.Headers.Add("X-Signature", Convert.ToBase64String(requestSignature));

                var response = await httpClient.PostAsync("https://localhost:7111/api/device/complete", httpContent);

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
