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
        private static readonly SecureRandom Random = new ();

        public Task<(MLDsaPublicKeyParameters PublicKey, MLDsaPrivateKeyParameters PrivateKey)> GenerateKeyPairAsync()
        {
            return Task.Run(() =>
            {
                var keyGen = new MLDsaKeyPairGenerator();
                keyGen.Init(new MLDsaKeyGenerationParameters(Random, MLDsaParameters.ml_dsa_87));
                var keyPair = keyGen.GenerateKeyPair();
                return ((MLDsaPublicKeyParameters)keyPair.Public, (MLDsaPrivateKeyParameters)keyPair.Private);
            });
        }

        public Task<byte[]> SignAsync(byte[] data, MLDsaPrivateKeyParameters privateKey)
        {
            return Task.Run(() =>
            {
                var signer = new MLDsaSigner(MLDsaParameters.ml_dsa_87, true);
                signer.Init(true, privateKey);
                signer.BlockUpdate(data, 0, data.Length);
                return signer.GenerateSignature();
            });
        }

        public Task<bool> VerifyAsync(byte[] publicKeyBytes, byte[] message, byte[] signature)
        {
            return Task.Run(() =>
            {
                var publicKey = MLDsaPublicKeyParameters.FromEncoding(MLDsaParameters.ml_dsa_87, publicKeyBytes);
                var verifier = new MLDsaSigner(MLDsaParameters.ml_dsa_87, false);
                verifier.Init(false, publicKey);
                verifier.BlockUpdate(message, 0, message.Length);
                return verifier.VerifySignature(signature);
            });
        }


        public Task<MLDsaPublicKeyParameters> RecoverPublicKeyAsync(byte[] publicKeyBytes)
        {
            var key = MLDsaPublicKeyParameters.FromEncoding(MLDsaParameters.ml_dsa_87, publicKeyBytes);
            return Task.FromResult(key);
        }

        public Task<MLDsaPrivateKeyParameters> RecoverPrivateKeyAsync(byte[] privateKeyBytes)
        {
            var key = MLDsaPrivateKeyParameters.FromEncoding(MLDsaParameters.ml_dsa_87, privateKeyBytes);
            return Task.FromResult(key);
        }
    }
}
