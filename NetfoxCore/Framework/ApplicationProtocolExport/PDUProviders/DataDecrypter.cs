using System;
using Netfox.Framework.ApplicationProtocolExport.Enums;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

namespace Netfox.Framework.ApplicationProtocolExport.PDUProviders
{
    internal abstract class DataDecrypter
    {
        protected IAeadBlockCipher CaeadCipher;
        protected BufferedBlockCipher Ccipher;

        protected BufferedStreamCipher CStreamCipher;

        protected IAeadBlockCipher SaeadCipher;

        protected BufferedBlockCipher Scipher;

        protected BufferedStreamCipher SStreamCipher;

        private Int32 _clientSeq;

        private Int32 _serverSeq;

        public IDigest Digest { get; set; }

        public Int32 KeyLength { get; set; }

        public Int32 IvLength { get; set; }

        public Int32 MacKeyLength { get; set; }

        public Int32 ClientSeq
        {
            get
            {
                var tmp = this._clientSeq;
                this._clientSeq++;
                return tmp;
            }
            set { this._clientSeq = value; }
        }

        public Int32 ServerSeq
        {
            get
            {
                var tmp = this._serverSeq;
                this._serverSeq++;
                return tmp;
            }
            set { this._serverSeq = value; }
        }

        public CipherMode Mode { get; protected set; }

        public Boolean Success { get; protected set; }

        public void DecreaseClientSeq()
        {
            this._clientSeq--;
        }

        public void DecreaseServerSeq()
        {
            this._serverSeq--;
        }

        public abstract Byte[] DecryptData(Byte[] cipheredData, PDUDecrypter.WriteKeySpec keySpec);

        public abstract void Init(Byte[] key);

        public abstract void SetGcmParameters(Byte[] exNonce, PDUDecrypter.WriteKeySpec writeKeySpec);

        public abstract void SetIv(Byte[] iv, PDUDecrypter.WriteKeySpec keySpec);
    }

    class AesDecrypter : DataDecrypter
    {
        private Byte[] _clientIv;

        private KeyParameter _clientKey;

        private Byte[] _serverIv;

        private KeyParameter _serverKey;

        public AesDecrypter(CipherMode mode)
        {
            this.Mode = mode;
            switch (this.Mode)
            {
                case CipherMode.Stream:
                    // AES is block cipher
                    throw new NotImplementedException();
                case CipherMode.Cbc:
                    this.Ccipher = new BufferedBlockCipher(new CbcBlockCipher(new AesEngine()));
                    this.Scipher = new BufferedBlockCipher(new CbcBlockCipher(new AesEngine()));
                    break;
                case CipherMode.Gcm:
                    this.Ccipher = null;
                    this.CaeadCipher = new GcmBlockCipher(new AesEngine());
                    this.Scipher = null;
                    this.SaeadCipher = new GcmBlockCipher(new AesEngine());
                    //throw new NotImplementedException();
                    break;
                case CipherMode.Ccm:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }

        public override Byte[] DecryptData(Byte[] cipheredData, PDUDecrypter.WriteKeySpec keySpec)
        {
            switch (this.Mode)
            {
                case CipherMode.Cbc:
                    if (keySpec == PDUDecrypter.WriteKeySpec.Client)
                    {
                        return this.Decrypt(this.Ccipher, cipheredData);
                    }

                    return this.Decrypt(this.Scipher, cipheredData);
                case CipherMode.Ccm:
                case CipherMode.Gcm:
                    if (keySpec == PDUDecrypter.WriteKeySpec.Client)
                    {
                        return this.Decrypt(this.CaeadCipher, cipheredData);
                    }

                    return this.Decrypt(this.SaeadCipher, cipheredData);
                default:
                    return null;
            }
        }

        public override void Init(Byte[] keyMaterial)
        {
            if (this.Mode != CipherMode.Cbc) return;

            /* client part */
            this._clientIv = new Byte[this.IvLength];
            Array.Copy(keyMaterial, this.MacKeyLength * 2 + this.KeyLength * 2, this._clientIv, 0, this.IvLength);
            this._clientKey = new KeyParameter(keyMaterial, this.MacKeyLength * 2, this.KeyLength);
            //Console.WriteLine("Client Key : "+ BitConverter.ToString(_clientKey.GetKey()));

            var keyAndIv = new ParametersWithIV(this._clientKey, this._clientIv, 0, this.IvLength);

            if (this.Mode == CipherMode.Cbc) // AEAD mode is initialized by other function
            {
                this.Ccipher.Reset();
                this.Ccipher.Init(false, keyAndIv);
            }

            /* server part */
            this._serverIv = new Byte[this.IvLength];
            Array.Copy(keyMaterial, this.MacKeyLength * 2 + this.KeyLength * 2 + this.IvLength, this._serverIv, 0,
                this.IvLength);
            this._serverKey = new KeyParameter(keyMaterial, this.MacKeyLength * 2 + this.KeyLength, this.KeyLength);

            //Console.WriteLine("Server Key : " + BitConverter.ToString(_serverKey.GetKey()));

            keyAndIv = new ParametersWithIV(this._serverKey, this._serverIv, 0, this.IvLength);

            if (this.Mode == CipherMode.Cbc)
            {
                this.Scipher.Reset();
                this.Scipher.Init(false, keyAndIv);
            }
        }

        public override void SetGcmParameters(Byte[] exNonce, PDUDecrypter.WriteKeySpec writeKeySpec)
        {
            var iv = writeKeySpec == PDUDecrypter.WriteKeySpec.Client ? this._clientIv : this._serverIv;

            var key = writeKeySpec == PDUDecrypter.WriteKeySpec.Client ? this._clientKey : this._serverKey;

            var cipher = writeKeySpec == PDUDecrypter.WriteKeySpec.Client ? this.CaeadCipher : this.SaeadCipher;

            /* Currently Bouncy Castle implementation of GCM supports only MAC size up to 128b */
            var par = new AeadParameters(key, this.Digest.GetDigestSize() * 8, iv, exNonce);

            cipher.Reset();
            cipher.Init(false, par);
        }

        public override void SetIv(Byte[] iv, PDUDecrypter.WriteKeySpec keySpec)
        {
            var key = keySpec == PDUDecrypter.WriteKeySpec.Client ? this._clientKey : this._serverKey;
            var cipher = keySpec == PDUDecrypter.WriteKeySpec.Client ? this.Ccipher : this.Scipher;

            var keyAndIv = new ParametersWithIV(key, iv);

            cipher.Reset();
            cipher.Init(false, keyAndIv);
        }

        private Byte[] Decrypt(BufferedBlockCipher cipher, Byte[] data)
        {
            var size = cipher.GetOutputSize(data.Length);
            var outBuffer = new Byte[size];
            //int off1 = cipher.ProcessBytes(content, 0, content.Length, outBuffer, 0);
            cipher.DoFinal(data, 0, data.Length, outBuffer, 0);
            //int off2 = cipher.DoFinal(outBuffer, off1);
            //int resultSize = off1 + off2;
            var result = new Byte[outBuffer.Length /*resultSize*/];
            Array.Copy(outBuffer, 0, result, 0, result.Length);

            //String asString = Encoding.ASCII.GetString(result);
            //Console.WriteLine(asString);

            return result;
        }

        private Byte[] Decrypt(IAeadBlockCipher cipher, Byte[] data)
        {
            var size = cipher.GetOutputSize(data.Length);
            var outBuffer = new Byte[size];
            var off1 = cipher.ProcessBytes(data, 0, data.Length, outBuffer, 0);
            //cipher.DoFinal(data, 0, data.Length, outBuffer, 0);
            var off2 = cipher.DoFinal(outBuffer, off1);
            var result = new Byte[off2];
            Array.Copy(outBuffer, 0, result, 0, result.Length);

            //String asString = Encoding.ASCII.GetString(result);
            //Console.WriteLine(asString);

            return result;
        }
    }

    class Rc4Decrypter : DataDecrypter
    {
        private KeyParameter _clientKey;

        private KeyParameter _serverKey;

        public Rc4Decrypter(CipherMode mode)
        {
            this.Mode = mode;
            switch (this.Mode)
            {
                case CipherMode.Stream:
                    this.CStreamCipher = new BufferedStreamCipher(new RC4Engine());
                    this.SStreamCipher = new BufferedStreamCipher(new RC4Engine());
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public override Byte[] DecryptData(Byte[] cipheredData, PDUDecrypter.WriteKeySpec keySpec)
        {
            if (keySpec == PDUDecrypter.WriteKeySpec.Client)
            {
                return this.Decrypt(this.CStreamCipher, cipheredData);
            }

            return this.Decrypt(this.SStreamCipher, cipheredData);
        }

        public override void Init(Byte[] keyMaterial)
        {
            /* client part */
            //_clientIV = new byte[IVLength];
            //Array.Copy(key_material, MacKeyLength * 2 + KeyLength * 2, _clientIV, 0, IVLength);
            this._clientKey = new KeyParameter(keyMaterial, this.MacKeyLength * 2, this.KeyLength);
            //Console.WriteLine("Client Key : "+ BitConverter.ToString(_clientKey.GetKey()));

            //_CStreamCipher.Reset();
            this.CStreamCipher.Init(false, this._clientKey);

            /* server part */
            //_serverIV = new byte[IVLength];
            //Array.Copy(key_material, MacKeyLength * 2 + KeyLength * 2 + IVLength, _serverIV, 0, IVLength);
            this._serverKey = new KeyParameter(keyMaterial, this.MacKeyLength * 2 + this.KeyLength, this.KeyLength);

            //Console.WriteLine("Server Key : " + BitConverter.ToString(_serverKey.GetKey()));

            //keyAndIv = new ParametersWithIV(_serverKey, _serverIV, 0, IVLength);

            //_SStreamCipher.Reset();
            this.SStreamCipher.Init(false, this._serverKey);
        }

        public override void SetGcmParameters(Byte[] exNonce, PDUDecrypter.WriteKeySpec writeKeySpec)
        {
            throw new NotImplementedException();
        }

        public override void SetIv(Byte[] iv, PDUDecrypter.WriteKeySpec keySpec)
        {
            throw new NotImplementedException();
        }

        private Byte[] Decrypt(BufferedStreamCipher cipher, Byte[] data)
        {
            var size = cipher.GetOutputSize(data.Length);
            var outBuffer = new Byte[size];
            //int off = cipher.DoFinal(data, outBuffer, 0);
            var off = cipher.ProcessBytes(data, 0, data.Length, outBuffer, 0);
            //cipher.DoFinal(data, 0, data.Length, outBuffer, 0);
            //cipher.DoFinal(outBuffer, off1);
            var result = new Byte[off];
            Array.Copy(outBuffer, 0, result, 0, result.Length);

            //String asString = Encoding.ASCII.GetString(result);
            //Console.WriteLine(asString);

            return result;
        }
    }
}