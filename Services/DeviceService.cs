using CoreLib.Crypto;
using CoreLib.Models.Entitys;
using Org.BouncyCastle.Crypto.Parameters;

namespace CoreLib.Services
{
    public interface IDeviceService
    {
        Task<(Device, MLDsaPrivateKeyParameters)> CreateAsync(string deviceName);
    }
    internal class DeviceService(IMLDsaKey mLDsaKey, IShakeGenerator shakeGenerator) : IDeviceService
    {
        public async Task<(Device, MLDsaPrivateKeyParameters)> CreateAsync(string deviceName)
        {
            try
            {
                var (mLDsaPK, MlDsaPrK) = await mLDsaKey.GenerateKeyPairAsync();

                Device device = new()
                {
                    DeviceName = deviceName,
                    SPK = mLDsaPK.GetEncoded(),
                    SPrK = MlDsaPrK.GetEncoded(),
                    CurrentDevice = true,
                };

                device.SPKSignature = await mLDsaKey.SignAsync(device.SPK, MlDsaPrK);

                device.Id = await shakeGenerator.GetString(await shakeGenerator.ComputeHash256(device.SPK, 64));

                return (device, MlDsaPrK);
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating device", ex);
            }
        }
    }
}
