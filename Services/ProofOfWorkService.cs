using CoreLib.Crypto;
using System.Security.Cryptography;

namespace CoreLib.Services
{
    public interface IProofOfWorkService
    {
        Task<(string nonce, string hash)> FindProofOfWork(string PK);
    }
    internal class ProofOfWorkService(IShakeGenerator shakeGenerator) : IProofOfWorkService
    {
        public async Task<(string nonce, string hash)> FindProofOfWork(string PK)
        {
            using var rng = RandomNumberGenerator.Create();
            var buffer = new byte[4];
            int attempts = 0;
            while (true)
            {
                rng.GetBytes(buffer);
                var nonce = BitConverter.ToUInt32(buffer, 0).ToString("X");
                string hash = Convert.ToBase64String(await shakeGenerator.ComputeHash128Async(PK + nonce)).ToLowerInvariant();

                if (hash.StartsWith("000"))
                    return (nonce, hash);

                attempts++;
                if (attempts > 10_000_000)
                    throw new Exception("PoW failed.");
            }
        }
    }
}
