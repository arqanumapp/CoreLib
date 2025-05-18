using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
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
        public async Task<byte[]> GenerateKey()
        {
            return await Task.Run(() =>
            {
                var key = new byte[256 / 8];
                new SecureRandom().NextBytes(key);
                return key;
            });
        }
        public async Task<byte[]> EncryptAsync(byte[] plaintext, byte[] key)
        {
            return await Task.Run(() =>
            {
                byte[] nonce = SecureRandom.GetNextBytes(new SecureRandom(), NonceSize);
                byte[] ciphertext = new byte[plaintext.Length + TagSize]; // фикс

                var cipher = new GcmBlockCipher(new Org.BouncyCastle.Crypto.Engines.AesEngine());
                var parameters = new AeadParameters(new KeyParameter(key), TagSize * 8, nonce);

                cipher.Init(true, parameters);
                int len = cipher.ProcessBytes(plaintext, 0, plaintext.Length, ciphertext, 0);
                cipher.DoFinal(ciphertext, len);

                byte[] result = new byte[NonceSize + ciphertext.Length];
                Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
                Buffer.BlockCopy(ciphertext, 0, result, NonceSize, ciphertext.Length);

                return result;
            });
        }

        public async Task<byte[]> DecryptAsync(byte[] encryptedData, byte[] key)
        {
            return await Task.Run(() =>
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

                return plaintext;
            });
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
