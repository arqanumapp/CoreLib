using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Security;

namespace CoreLib.Crypto
{
    public interface IMLDsaKey
    {
        Task<(MLDsaPublicKeyParameters PublicKey, MLDsaPrivateKeyParameters PrivateKey)> GenerateKeyPairAsync();
        Task<byte[]> SignAsync(byte[] data, MLDsaPrivateKeyParameters privateKey);
        Task<bool> VerifyAsync(byte[] publicKeyBytes, byte[] message, byte[] signature);
        Task<MLDsaPublicKeyParameters> RecoverPublicKeyAsync(byte[] publicKeyBytes);
        Task<MLDsaPrivateKeyParameters> RecoverPrivateKeyAsync(byte[] privateKeyBytes);
    }
    internal class MLDsaKey : IMLDsaKey
    {
        private SecureRandom Random = new SecureRandom();

        public async Task<(MLDsaPublicKeyParameters PublicKey, MLDsaPrivateKeyParameters PrivateKey)> GenerateKeyPairAsync()
        {
            return await Task.Run(() =>
            {
                var keyGen = new MLDsaKeyPairGenerator();
                keyGen.Init(new MLDsaKeyGenerationParameters(Random, MLDsaParameters.ml_dsa_87));
                AsymmetricCipherKeyPair keyPair = keyGen.GenerateKeyPair();
                return ((MLDsaPublicKeyParameters)keyPair.Public, (MLDsaPrivateKeyParameters)keyPair.Private);
            });
        }

        public async Task<byte[]> SignAsync(byte[] data, MLDsaPrivateKeyParameters privateKey)
        {
            return await Task.Run(() =>
            {
                var signer = new MLDsaSigner(MLDsaParameters.ml_dsa_87, true);
                signer.Init(true, privateKey);
                signer.BlockUpdate(data, 0, data.Length);
                return signer.GenerateSignature();
            });
        }

        public async Task<bool> VerifyAsync(byte[] publicKeyBytes, byte[] message, byte[] signature)
        {
            return await Task.Run(async () =>
            {
                var publicKey = await RecoverPublicKeyAsync(publicKeyBytes);
                var verifier = new MLDsaSigner(MLDsaParameters.ml_dsa_87, false);
                verifier.Init(false, publicKey);
                verifier.BlockUpdate(message, 0, message.Length);
                return verifier.VerifySignature(signature);
            });
        }



        public async Task<MLDsaPublicKeyParameters> RecoverPublicKeyAsync(byte[] publicKeyBytes)
        {
            return await Task.Run(() =>
            {
                return MLDsaPublicKeyParameters.FromEncoding(MLDsaParameters.ml_dsa_87, publicKeyBytes);
            });
        }

        public async Task<MLDsaPrivateKeyParameters> RecoverPrivateKeyAsync(byte[] privateKeyBytes)
        {
            return await Task.Run(() =>
            {
                return MLDsaPrivateKeyParameters.FromEncoding(MLDsaParameters.ml_dsa_87, privateKeyBytes);
            });
        }

    }
}
