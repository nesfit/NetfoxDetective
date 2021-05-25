using System;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;

namespace Netfox.Framework.ApplicationProtocolExport.PDUProviders
{
    internal abstract class KeyDecrypter
    {
        protected IAsymmetricBlockCipher Cipher;
        protected RsaKeyParameters PrivateKey;
        public abstract Byte[] DecryptKey();
        public abstract void ParseClientKeyExchange(Byte[] message);
    }
}