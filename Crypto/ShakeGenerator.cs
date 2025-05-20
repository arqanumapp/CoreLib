using Org.BouncyCastle.Crypto.Digests;
using System.Text;

namespace CoreLib.Crypto
{
    public interface IShakeGenerator
    {
        Task<byte[]> ComputeHash256Async(byte[] input, int outputLength = 32);
        Task<byte[]> ComputeHash128Async(string input);
        Task<string> ToBase64StringAsync(byte[] bytes);
    }

    internal class ShakeGenerator : IShakeGenerator
    {
        // SHAKE256 хэш
        public Task<byte[]> ComputeHash256Async(byte[] input, int outputLength = 32)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (outputLength <= 0) throw new ArgumentOutOfRangeException(nameof(outputLength));

            return Task.Run(() =>
            {
                var digest = new ShakeDigest(256);
                digest.BlockUpdate(input, 0, input.Length);
                var output = new byte[outputLength];
                digest.DoFinal(output, 0);
                return output;
            });
        }

        // SHAKE128 хэш для строки UTF-8
        public Task<byte[]> ComputeHash128Async(string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            return Task.Run(() =>
            {
                var shake = new ShakeDigest(128);
                var inputBytes = Encoding.UTF8.GetBytes(input);
                shake.BlockUpdate(inputBytes, 0, inputBytes.Length);
                var output = new byte[32]; // фиксированная длина 32 байта
                shake.DoFinal(output, 0);
                return output;
            });
        }

        public Task<string> ToBase64StringAsync(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            return Task.FromResult(Convert.ToBase64String(bytes));
        }
    }
}
