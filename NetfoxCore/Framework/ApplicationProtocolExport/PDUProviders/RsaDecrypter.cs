using System;
using System.IO;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;

namespace Netfox.Framework.ApplicationProtocolExport.PDUProviders
{
    internal class RsaDecrypter : KeyDecrypter
    {
        private Byte[] _encryptedPreMaster;
        //private RSACryptoServiceProvider _RSAAlg;

        public RsaDecrypter(String pk)
        {
            //_RSAAlg = new RSACryptoServiceProvider();
            //RSAParameters rsap = new RSAParameters();
            using (TextReader tr = new StringReader(pk))
            {
                //byte[] bytes = new byte[pk.Length*sizeof (char)];
                //System.Buffer.BlockCopy(pk.ToCharArray(), 0, bytes, 0, bytes.Length);

                AsymmetricCipherKeyPair keyPair;
                var pemReader = new PemReader(tr);
                keyPair = (AsymmetricCipherKeyPair) pemReader.ReadObject();

                this.PrivateKey = (RsaKeyParameters) keyPair.Private;

                //Console.WriteLine(_privateKey.ToString());

                this.Cipher = new RsaEngine();
                this.Cipher.Init(false, this.PrivateKey);
            }
        }

        public override Byte[] DecryptKey()
        {
            var decryptedPremaster =
                this.Cipher.ProcessBlock(this._encryptedPreMaster, 0, this._encryptedPreMaster.Length);

            var pad = 0;
            for (var i = 0; i < decryptedPremaster.Length; i++)
            {
                if (decryptedPremaster[i] == 0)
                {
                    pad = i + 1;
                    break;
                }
            }

            var strippedPremaster = new Byte[decryptedPremaster.Length - pad];

            Array.Copy(decryptedPremaster, pad, strippedPremaster, 0, strippedPremaster.Length);

            return strippedPremaster;
        }

        public override void ParseClientKeyExchange(Byte[] message)
        {
            //var messageAsStringArray = Convert.ToString(message).Split('-');
            var len = new Byte[2];
            Array.Copy(message, 9, len, 0, 2);
            Array.Reverse(len);
            var preMasterLen = BitConverter.ToInt16(len, 0);
            this._encryptedPreMaster = new Byte[preMasterLen];
            Array.Copy(message, 11, this._encryptedPreMaster, 0, preMasterLen);
        }

        public void SetEncryptedPreMaster(Byte[] premaster)
        {
            this._encryptedPreMaster = premaster;
        }
    }
}