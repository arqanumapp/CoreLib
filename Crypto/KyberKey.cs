using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace CoreLib.Crypto
{
    internal class KyberKey
    {
        public async Task<(MLKemPublicKeyParameters publicKey, MLKemPrivateKeyParameters privateKey)> GenerateKeyPairAsync()
        {
            return await Task.Run(() =>
            {
                SecureRandom random = new SecureRandom();
                var kemKpg = new MLKemKeyPairGenerator();
                kemKpg.Init(new MLKemKeyGenerationParameters(random, MLKemParameters.ml_kem_1024));

                AsymmetricCipherKeyPair kemKp = kemKpg.GenerateKeyPair();

                var kemPublicKey = (MLKemPublicKeyParameters)kemKp.Public;
                var kemPrivateKey = (MLKemPrivateKeyParameters)kemKp.Private;

                return ((MLKemPublicKeyParameters)kemKp.Public, (MLKemPrivateKeyParameters)kemKp.Private);
            });
        }

        public async Task<(byte[] kemCipherText, byte[] sharedSecret)> EncapsulateAsync(byte[] receiverPublicKeyBytes)
        {
            return await Task.Run(() =>
            {
                AsymmetricKeyParameter publicKey = PublicKeyFactory.CreateKey(receiverPublicKeyBytes);

                var encapsulator = KemUtilities.GetEncapsulator("ML-KEM-1024");
                encapsulator.Init(publicKey);

                byte[] kemCipherText = new byte[encapsulator.EncapsulationLength];
                byte[] sharedSecret = new byte[encapsulator.SecretLength];

                encapsulator.Encapsulate(kemCipherText, 0, kemCipherText.Length, sharedSecret, 0, sharedSecret.Length);

                return (kemCipherText, sharedSecret);
            });
        }

        public async Task<byte[]> DecapsulateAsync(byte[] kemCipherText, byte[] privateKey)
        {
            return await Task.Run(() =>
            {
                var decapsulator = KemUtilities.GetDecapsulator("ML-KEM-1024");
                var encryptionPrivateKeyObj = MLKemPrivateKeyParameters.FromEncoding(MLKemParameters.ml_kem_1024, privateKey);
                decapsulator.Init(encryptionPrivateKeyObj);

                byte[] sharedSecret = new byte[decapsulator.SecretLength];

                decapsulator.Decapsulate(kemCipherText, 0, kemCipherText.Length, sharedSecret, 0, sharedSecret.Length);

                return sharedSecret;
            });
        }
    }
}
