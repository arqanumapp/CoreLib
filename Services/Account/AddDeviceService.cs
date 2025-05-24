using CoreLib.Crypto;
using CoreLib.Interfaces;
using CoreLib.Models.Dtos.Device.Add;
using CoreLib.Models.Entitys;
using CoreLib.Models.Entitys.Devices;
using CoreLib.Sockets;
using CoreLib.Storage;
using MessagePack;
using System.Text;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CoreLib.Services.Account
{
    public interface IAddDeviceService
    {
        Task<bool> StartDeviceProvisioningAsync(string deviceName, byte[] aesKeyBytes);
        Task<(string qr, Task listeningTask)> GetDeviceQrData(Func<bool, Task>? onComplete = null, CancellationToken cancellationToken = default);
    }
    internal class AddDeviceService(
        IDeviceService deviceService,
        IAesGCMKey aesGCMKey,
        IDeviceInfoProvider deviceInfoProvider,
        IPreKeyService preKeyService,
        IAccountStorage accountStorage,
        IDeviceStorage deviceStorage,
        IShakeGenerator shakeGenerator,
        IMLDsaKey mLDsaKey,
        IPreKeyStorage preKeyStorage,
        IApiService apiService) : IAddDeviceService
    {
        public async Task<bool> StartDeviceProvisioningAsync(string deviceName, byte[] aesKeyBytes)
        {
            try
            {
                var (device, SPrKSignatire, mLDsaPrK) = await deviceService.CreateAsync(deviceName);

                var account = await accountStorage.GetAccountAsync() ?? throw new ArgumentNullException("Account not found");
                var currentDevice = await deviceStorage.GetCurrentDevice() ?? throw new ArgumentNullException("Current device not found");
                var privatePayload = new NewDevicePrivatePayloadRequest
                {
                    Name = deviceName,
                    AccountId = account.Id,
                    DeviceId = device.Id,
                    SPK = device.DeviceKeys.SPK,
                    SPrK = device.DeviceKeys.SPrK,
                };

                var publicPayload = new NewDevicePublicPayloadRequest
                {
                    Name = deviceName,
                    AccountId = account.Id,
                    TrustedDeviceId = currentDevice.Id,
                    DeviceId = device.Id,
                    SPK = device.DeviceKeys.SPK,
                    SPKSignature = SPrKSignatire,
                };

                byte[] rawEncryptedPrivatePayload = await aesGCMKey.EncryptAsync(MessagePackSerializer.Serialize(privatePayload), aesKeyBytes);

                byte[] rawPublicPayload = MessagePackSerializer.Serialize(publicPayload);
                byte[] rawEncryptedPublicPayload = await aesGCMKey.EncryptAsync(rawPublicPayload, aesKeyBytes);

                var currentSPrK = await mLDsaKey.RecoverPrivateKeyAsync(currentDevice.DeviceKeys.SPrK);

                NewDeviceRequest request = new();

                request.PrivatePayload = rawEncryptedPrivatePayload;
                request.PublicPayload = rawEncryptedPublicPayload;
                request.TempId = Convert.ToBase64String(await shakeGenerator.ComputeHash256Async(aesKeyBytes, 64));
                request.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                request.TrustedSignature = await aesGCMKey.EncryptAsync(await mLDsaKey.SignAsync(rawPublicPayload, currentSPrK), aesKeyBytes);
                request.PrivatePayloadHash = await shakeGenerator.ComputeHash256Async(rawEncryptedPrivatePayload, 64);
                request.PublicPayloadHash = await shakeGenerator.ComputeHash256Async(rawEncryptedPublicPayload, 64);
                request.TrustedDeviceId = currentDevice.Id;

                var response = await apiService.PostAsync(request, currentSPrK, "device/add");

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> ConfirmDeviceProvisioningAsync(byte[] responseData, byte[] aesKeyBytes)
        {
            try
            {
                var provisioningResponse = MessagePackSerializer.Deserialize<NewDeviceResponce>(responseData);

                var computedPublicHash = await shakeGenerator.ComputeHash256Async(provisioningResponse.PublicPayload, 64);
                var computedPrivateHash = await shakeGenerator.ComputeHash256Async(provisioningResponse.PrivatePayload, 64);

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

                var request = new AddNewDeviceRequest
                {
                    DevicePayload = publicPayloadBytes,

                    PreKeysPayload = [.. preKeys.Select(x => new AddNewDevicePreKeysRequest
                        {
                            Id = x.Id,
                            PK = x.PK,
                            PKSignature = x.Signature
                        })],

                    DeviceTrustedSignature = trustedSignatureBytes
                };
                request.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var response = await apiService.PostAsync(request, deviceSPrK, "device/confirm");

                if (response.IsSuccessStatusCode)
                {
                    await accountStorage.SaveAccountAsync(new Models.Entitys.Account
                    {
                        Id = publicPayload.AccountId,
                        NickName = publicPayload.Name,
                    });

                    await deviceStorage.SaveDeviceAsync(new Device
                    {
                        Id = publicPayload.DeviceId,
                        DeviceName = publicPayload.Name,
                        DeviceKeys = new DeviceKeys
                        {
                            SPK = publicPayload.SPK,
                            SPrK = privatePayload.SPrK,
                        },
                        AccountId = publicPayload.AccountId,
                    });

                    await preKeyStorage.AddRangeAsync(preKeys);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<(string qr, Task listeningTask)> GetDeviceQrData(Func<bool, Task>? onComplete = null, CancellationToken cancellationToken = default)
        {
            var aesKey = await aesGCMKey.GenerateKey();
            var deviceName = await deviceInfoProvider.GetDeviceName();

            string channelId = Convert.ToBase64String(await shakeGenerator.ComputeHash256Async(aesKey, 64));
            var listener = new DeviceProvisioningListener();

            var isStopped = false;

            var listeningTask = Task.Run(async () =>
            {
                try
                {
                    await listener.StartListeningAsync(channelId, async (responseData) =>
                    {
                        if (cancellationToken.IsCancellationRequested)
                            return;

                        bool result = await ConfirmDeviceProvisioningAsync(responseData, aesKey);

                        if (onComplete != null)
                            await onComplete(result);

                        await listener.StopAsync();
                    });

                    cancellationToken.Register(async () => await listener.StopAsync());
                }
                catch (Exception ex)
                {
                }
            });

            

            // Настройки сериализации: без лишних пробелов
            var options = new JsonSerializerOptions
            {
                WriteIndented = false
            };

            // Сериализация
            string json = JsonSerializer.Serialize(new AddDeviceQrCore
            {
                Name = deviceName,
                Key = Convert.ToBase64String(aesKey)
            }, options);

            // Преобразование в Base64
            return (json, listeningTask);
        }


    }
}
