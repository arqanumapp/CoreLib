using CoreLib.Crypto;
using System.Security.Cryptography;

namespace CoreLib.Services
{
    internal class ProofOfWorkService
    {
        public static async Task<(string nonce, string hash)> FindProofOfWork(string publicKey, int difficulty = 1)
        {
            using var rng = RandomNumberGenerator.Create();
            var buffer = new byte[4];
            int attempts = 0;
            var shakeGenerator = new ShakeGenerator();
            while (true)
            {
                rng.GetBytes(buffer);
                var nonce = BitConverter.ToUInt32(buffer, 0).ToString("X");
                string hash = Convert.ToHexString(await shakeGenerator.ComputeHash128(publicKey + nonce)).ToLowerInvariant();

                if (hash.StartsWith(new string('0', difficulty)))
                    return (nonce, hash);

                attempts++;
                if (attempts > 10_000_000)
                    throw new Exception("PoW failed.");
            }
        }
    }
}
