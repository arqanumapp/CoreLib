using CoreLib.Crypto;
using CoreLib.Models.Entitys;
using Org.BouncyCastle.Crypto.Parameters;

namespace CoreLib.Services
{
    internal class DeviceService
    {
        public async Task<(Device, MLDsaPrivateKeyParameters)> CreateAsync(string deviceName)
        {
            try
            {
                Device device = new();
                device.DeviceName = deviceName;

                var preKeyService = new PreKeyService();

                var dilithiumKey = new DilitiumKey();

                var (dilithiumPublicKey, dilithiumPrivateKey) = await dilithiumKey.GenerateKeyPairAsync();

                device.SPK = dilithiumPublicKey.GetEncoded();

                device.SPrK = dilithiumPrivateKey.GetEncoded();

                device.SPKSignature = await dilithiumKey.SignAsync(device.SPK, dilithiumPrivateKey);
                var shakeGenerator = new ShakeGenerator();

                var id = await shakeGenerator.ComputeHash256(device.SPK, 64);
                device.Id = await shakeGenerator.GetString(id);

                return (device, dilithiumPrivateKey);


            }
            catch (Exception ex)
            {
                throw new Exception("Error creating device", ex);
            }
        }
    }
}
