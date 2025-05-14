using CoreLib.Crypto;
using CoreLib.Models.Entitys;
using Org.BouncyCastle.Crypto.Parameters;

namespace CoreLib.Services
{
    internal class PreKeyService
    {
        public async Task<PreKey> CreateAsync(MLDsaPrivateKeyParameters dilithiumPrivateKey, string deviceId)
        {
            try
            {
                var kyberKey = new KyberKey();
                var shakeGen = new ShakeGenerator();
                var dilithiumKey = new DilitiumKey();
                var (publicKey, privateKey) = await kyberKey.GenerateKeyPairAsync();
                var preKey = new PreKey
                {
                    Id = await shakeGen.GetString(await shakeGen.ComputeHash256(publicKey.GetEncoded(),64)),
                    PK = publicKey.GetEncoded(),
                    PrK = privateKey.GetEncoded(),
                    Signature = await dilithiumKey.SignAsync(publicKey.GetEncoded(), dilithiumPrivateKey)
                };
                return preKey;
            }
            catch (Exception ex)
            {
                throw new Exception("Error generating prekey", ex);
            }
        }
    }
}
