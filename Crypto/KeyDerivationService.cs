using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using System.Text;

namespace CoreLib.Crypto
{
    public interface IKeyDerivationService
    {
        byte[] DeriveAesKeyKMAC256(byte[] sharedSecret, int keySizeBytes = 32, string customization = "", byte[] context = null);
    }
    internal class KeyDerivationService : IKeyDerivationService
    {
        public byte[] DeriveAesKeyKMAC256(byte[] sharedSecret, int keySizeBytes = 32, string customization = "", byte[] context = null)
        {
            context ??= [];

            var kmac = new KMac(256, Encoding.UTF8.GetBytes(customization));
            kmac.Init(new KeyParameter(sharedSecret));

            kmac.BlockUpdate(context, 0, context.Length);

            var output = new byte[keySizeBytes];
            kmac.DoFinal(output, 0);

            return output;
        }

    }
}
