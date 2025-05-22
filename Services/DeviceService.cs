using CoreLib.Crypto;
using CoreLib.Models.Entitys.Devices;
using Org.BouncyCastle.Crypto.Parameters;

namespace CoreLib.Services
{
    public interface IDeviceService
    {
        Task<(Device device, byte[] SPKSignature, MLDsaPrivateKeyParameters mlDsaPrK)> CreateAsync(string deviceName);
    }
    internal class DeviceService(IMLDsaKey mLDsaKey, IShakeGenerator shakeGenerator, INotificationService notificationService) : IDeviceService
    {
        public async Task<(Device device, byte[] SPKSignature, MLDsaPrivateKeyParameters mlDsaPrK)> CreateAsync(string deviceName)
        {
            try
            {
                var (mLDsaPK, MlDsaPrK) = await mLDsaKey.GenerateKeyPairAsync();

                Device device = new()
                {
                    DeviceName = deviceName,
                    DeviceKeys = new DeviceKeys
                    {
                        SPK = mLDsaPK.GetEncoded(),
                        SPrK = MlDsaPrK.GetEncoded(),
                    },
                };

                byte [] SPKSignature = await mLDsaKey.SignAsync(device.DeviceKeys.SPK, MlDsaPrK);

                device.Id = await shakeGenerator.ToBase64StringAsync(await shakeGenerator.ComputeHash256Async(device.DeviceKeys.SPK, 64));

                return (device, SPKSignature, MlDsaPrK);
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating device", ex);
            }
        }
    }
}
