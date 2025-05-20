using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System.Text;

namespace CoreLib.Crypto
{
    public interface IAesGCMKey
    {
        Task<byte[]> EncryptAsync(byte[] plaintext, byte[] key);
        Task<byte[]> DecryptAsync(byte[] encryptedData, byte[] key);
        Task<byte[]> EncryptString(string message, byte[] key);
        Task<string> DecryptToString(byte[] encryptedData, byte[] key);
        Task<byte[]> GenerateKey();
    }

    internal class AesGCMKey : IAesGCMKey
    {
        private const int NonceSize = 12;
        private const int TagSize = 16;
        private static readonly SecureRandom SecureRng = new();

        public Task<byte[]> GenerateKey()
        {
            var key = new byte[32];
            SecureRng.NextBytes(key);
            return Task.FromResult(key);
        }

        public Task<byte[]> EncryptAsync(byte[] plaintext, byte[] key)
        {
            byte[] nonce = new byte[NonceSize];
            SecureRng.NextBytes(nonce);

            byte[] ciphertext = new byte[plaintext.Length + TagSize];

            var cipher = new GcmBlockCipher(new Org.BouncyCastle.Crypto.Engines.AesEngine());
            var parameters = new AeadParameters(new KeyParameter(key), TagSize * 8, nonce);

            cipher.Init(true, parameters);
            int len = cipher.ProcessBytes(plaintext, 0, plaintext.Length, ciphertext, 0);
            cipher.DoFinal(ciphertext, len);

            byte[] result = new byte[NonceSize + ciphertext.Length];
            Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
            Buffer.BlockCopy(ciphertext, 0, result, NonceSize, ciphertext.Length);

            return Task.FromResult(result);
        }

        public Task<byte[]> DecryptAsync(byte[] encryptedData, byte[] key)
        {
            byte[] nonce = new byte[NonceSize];
            byte[] ciphertext = new byte[encryptedData.Length - NonceSize];

            Buffer.BlockCopy(encryptedData, 0, nonce, 0, NonceSize);
            Buffer.BlockCopy(encryptedData, NonceSize, ciphertext, 0, ciphertext.Length);

            var cipher = new GcmBlockCipher(new Org.BouncyCastle.Crypto.Engines.AesEngine());
            var parameters = new AeadParameters(new KeyParameter(key), TagSize * 8, nonce);

            cipher.Init(false, parameters);
            byte[] plaintext = new byte[ciphertext.Length];
            int len = cipher.ProcessBytes(ciphertext, 0, ciphertext.Length, plaintext, 0);
            cipher.DoFinal(plaintext, len);

            return Task.FromResult(plaintext);
        }

        public async Task<byte[]> EncryptString(string message, byte[] key)
        {
            var data = Encoding.UTF8.GetBytes(message);
            return await EncryptAsync(data, key);
        }

        public async Task<string> DecryptToString(byte[] encryptedData, byte[] key)
        {
            var data = await DecryptAsync(encryptedData, key);
            return Encoding.UTF8.GetString(data);
        }
    }
}
