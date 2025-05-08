using Org.BouncyCastle.Crypto.Digests;

namespace CoreLib.Crypto
{
    internal class Shake256Generator
    {
        public async Task<byte[]> ComputeHash(byte[] input, int outputLength = 32)
        {
            return await Task.Run(() =>
            {
                var digest = new ShakeDigest(256);
                digest.BlockUpdate(input, 0, input.Length);
                byte[] output = new byte[outputLength];
                digest.DoFinal(output, 0);
                return output;
            });
        }

        public async Task<string> GetHex(byte[] bytes) => await Task.Run(() =>
        {
            return Convert.ToHexString(bytes).ToLowerInvariant();
        });
    }
}
