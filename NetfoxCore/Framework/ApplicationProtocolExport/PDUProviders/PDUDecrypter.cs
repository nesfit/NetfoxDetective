using System;
using System.Linq;
using System.Text;
using Netfox.Framework.ApplicationProtocolExport.Enums;
using Netfox.Framework.Models.Enums;
using Netfox.Framework.Models.PmLib;
using Netfox.Framework.Models.PmLib.Frames;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;

namespace Netfox.Framework.ApplicationProtocolExport.PDUProviders
{
    public class PDUDecrypter
    {
        public enum WriteKeySpec
        {
            Client,
            Server
        }

        // Version Minor
        public const Byte Ssl3 = 0;
        public const Byte Tls10 = 1;
        public const Byte Tls11 = 2;
        public const Byte Tls12 = 3;

        public Byte[] ClientDirection;

        public Byte[] ClientHMacKey;

        public Byte[] ClientRnd;

        public Byte[] ContinuationData;

        public Byte[] ServerHMacKey;

        public Byte[] ServerRnd;

        internal KeyDecrypter KeyDecrypter { get; set; }

        internal DataDecrypter DataDecrypter { get; set; }

        public void ChangeCipherSuite(Byte[] data, out ConversationCipherSuite cipherSuite)
        {
            try
            {
                var len = new Byte[2];
                Array.Copy(data, 3, len, 0, 2);
                Array.Reverse(len);

                // 1 byte for type
                // 2 bytes tls/ssl version
                // 2 bytes PDU length
                var offset = 5;

                len = new Byte[4];
                // only 3 bytes are used so offset is 1
                Array.Copy(data, offset + 1, len, 1, 3);
                Array.Reverse(len);

                // Should be server hello message

                // 71-72 position of bytes in Server hello message that inform about ciphersuite
                len = new Byte[2];
                Array.Copy(data, offset + 71, len, 0, 2);
                Array.Reverse(len);
                var cs = BitConverter.ToInt16(len, 0);
                cipherSuite = (ConversationCipherSuite) cs;
            }
            catch (IndexOutOfRangeException e)
            {
                PmConsolePrinter.PrintError("PDUDecrypter : Problem when parsing ServerHello : " + e.Message);
                cipherSuite = ConversationCipherSuite.TlsNullWithNullNull;
            }
        }

        public Boolean DoDecryption(Byte[] pdu, PmFrameBase frame, ref Byte[] decryptedBytes)
        {
            var source = frame.SrcAddress.GetAddressBytes();
            var bytes = pdu;

            var hmac = new HMac(this.DataDecrypter.Digest);

            // get content type - protocol id
            var ct = new Byte[1];
            Array.Copy(bytes, 0, ct, 0, 1);

            // get version - tls version next two bytes
            var version = new Byte[2];
            Array.Copy(bytes, 1, version, 0, 2);

            // get data length 
            var len = new Byte[2];
            Array.Copy(bytes, 3, len, 0, 2);

            Array.Reverse(len);
            Int32 dataLen = BitConverter.ToInt16(len, 0);

            // get data
            var data = new Byte[dataLen];

            // not whole data - continuation in next frame
            if (bytes.Length < dataLen + 5)
            {
                this.ContinuationData = new Byte[bytes.Length];
                Array.Copy(bytes, this.ContinuationData, this.ContinuationData.Length);
                return false;
            }

            // more than enough data - new frame starts here 
            if (bytes.Length > dataLen + 5)
            {
                this.ContinuationData = new Byte[bytes.Length - dataLen - 5];
                Array.Copy(bytes, dataLen + 5, this.ContinuationData, 0, this.ContinuationData.Length);
            }

            Array.Copy(bytes, 5, data, 0, dataLen);

            /* decryption */

            var sequenceNum = 0;
            Byte[] decrypted = null;

            try
            {
                switch (this.DataDecrypter.Mode)
                {
                    case CipherMode.Cbc:
                        decrypted = this.BlockDecryption(source, data, hmac, out sequenceNum);
                        break;
                    case CipherMode.Stream:
                        decrypted = this.StreamDecryption(source, data, hmac, out sequenceNum);
                        break;
                    case CipherMode.Ccm:
                        return false;
                    case CipherMode.Gcm:
                        decrypted = this.AeadDecryption(source, data);
                        break;
                }
            }
            catch (InvalidCipherTextException e)
            {
                ("PDUDecryter : " + e.Message).PrintInfo();
                return false;
            }


            /* check if decrypted data are correct */

            /* GCM has no mac */
            if (this.DataDecrypter.Mode == CipherMode.Gcm) return true;

            var msgLen = decrypted.Length;
            // strip padding - if CBC
            if (this.DataDecrypter.Mode == CipherMode.Cbc)
            {
                var pad = decrypted[decrypted.Length - 1];
                msgLen -= pad + 1;
            }

            // strip mac
            msgLen -= this.DataDecrypter.Digest.GetDigestSize();

            if (msgLen < 1) return false;

            var msg = new Byte[msgLen];
            Array.Copy(decrypted, msg, msgLen);

            var recvDigest = new Byte[this.DataDecrypter.Digest.GetDigestSize()];
            Array.Copy(decrypted, msgLen, recvDigest, 0, recvDigest.Length);

            var ignoreHmac = true;
            if (this.tls_hmac_check(sequenceNum, hmac, ct, version, msgLen, msg, recvDigest) || ignoreHmac)
            {
                Byte[] tmp = null;
                if (decryptedBytes != null)
                {
                    tmp = new Byte[decryptedBytes.Length];
                    Array.Copy(decryptedBytes, tmp, tmp.Length);
                    decryptedBytes = new Byte[tmp.Length + msg.Length];
                    Array.Copy(tmp, decryptedBytes, tmp.Length);
                    Array.Copy(msg, 0, decryptedBytes, tmp.Length, msg.Length);
                }
                else
                {
                    decryptedBytes = new Byte[msg.Length];
                    Array.Copy(msg, decryptedBytes, msg.Length);
                }


                return true;
            }

            // If bad decrypted decrease sequence number
            if (Enumerable.SequenceEqual(source, this.ClientDirection))
            {
                this.DataDecrypter.DecreaseClientSeq();
            }
            else
            {
                this.DataDecrypter.DecreaseServerSeq();
            }

            return false;
        }

        public Byte[] Prf(Byte[] secret, String label, Byte[] rnd1, Byte[] rnd2, Int32 size, Byte version)
        {
            var ret = new Byte[size];

            var bLabel = Encoding.ASCII.GetBytes(label);

            var seed = new Byte[bLabel.Length + rnd1.Length + rnd2.Length];

            Array.Copy(bLabel, 0, seed, 0, bLabel.Length);

            Array.Copy(rnd1, 0, seed, bLabel.Length, rnd1.Length);

            Array.Copy(rnd2, 0, seed, bLabel.Length + rnd1.Length, rnd2.Length);

            switch (version)
            {
                case Ssl3:
                    // TODO
                    throw new NotImplementedException();
                case Tls10:
                case Tls11:
                    /* PRF is result of mixing two hashes */
                    var s1 = new Byte[(Int32) Math.Ceiling(secret.Length / 2.0)];
                    Array.Copy(secret, s1, s1.Length);
                    var s2 = new Byte[s1.Length];
                    Array.Copy(secret, secret.Length - s2.Length, s2, 0, s2.Length);
                    var md5Hash = new Byte[ret.Length];
                    var sha1Hash = new Byte[ret.Length];
                    md5Hash = this.P_hash(s1, seed, size, new MD5Digest());
                    sha1Hash = this.P_hash(s2, seed, size, new Sha1Digest());
                    for (var i = 0; i < size; i++) ret[i] = (Byte) (md5Hash[i] ^ sha1Hash[i]);
                    break;
                case Tls12:
                    /* All Cipher suites defined in TLS 1.2 RFC
                     * uses sha256 in this function
                     */
                    ret = this.P_hash(secret, seed, size, new Sha256Digest());
                    break;
            }

            return ret;
        }

        private Byte[] AeadDecryption(Byte[] source, Byte[] data)
        {
            Byte[] decrypted;
            var exNonce = new Byte[8];
            var content = new Byte[data.Length - 8];

            Array.Copy(data, 0, exNonce, 0, 8);
            Array.Copy(data, 8, content, 0, content.Length);

            if (source.SequenceEqual(this.ClientDirection))
            {
                this.DataDecrypter.SetGcmParameters(exNonce, WriteKeySpec.Client);
                var d = this.DataDecrypter.DecryptData(content, WriteKeySpec.Client);

                decrypted = new Byte[d.Length];

                Array.Copy(d, decrypted, d.Length);
            }
            else
            {
                this.DataDecrypter.SetGcmParameters(exNonce, WriteKeySpec.Server);
                var d = this.DataDecrypter.DecryptData(data, WriteKeySpec.Server);

                decrypted = new Byte[d.Length];

                Array.Copy(d, decrypted, d.Length);
            }

            return decrypted;
        }

        private Byte[] BlockDecryption(Byte[] source, Byte[] data, HMac hmac, out Int32 sequenceNum)
        {
            // RFC 5246 in tls 1.2 ciphertext consits of
            // iv, ciphered_content { content, mac, padding, padding_length }
            // the IV length is of length
            // record_iv_length, which is equal to the
            // block_size.

            var content = new Byte[data.Length - this.DataDecrypter.IvLength];

            var iv = new Byte[this.DataDecrypter.IvLength];

            Array.Copy(data, 0, iv, 0, this.DataDecrypter.IvLength);

            Array.Copy(data, this.DataDecrypter.IvLength, content, 0, content.Length);

            Byte[] decrypted;

            if (source.SequenceEqual(this.ClientDirection))
            {
                this.DataDecrypter.SetIv(iv, WriteKeySpec.Client);
                var d = this.DataDecrypter.DecryptData(content, WriteKeySpec.Client);

                decrypted = new Byte[d.Length];

                Array.Copy(d, decrypted, d.Length);

                var key = new KeyParameter(this.ClientHMacKey);
                hmac.Init(key);

                sequenceNum = this.DataDecrypter.ClientSeq;
            }
            else
            {
                this.DataDecrypter.SetIv(iv, WriteKeySpec.Server);
                var d = this.DataDecrypter.DecryptData(content, WriteKeySpec.Server);

                decrypted = new Byte[d.Length];

                Array.Copy(d, decrypted, d.Length);

                var key = new KeyParameter(this.ServerHMacKey);
                hmac.Init(key);

                sequenceNum = this.DataDecrypter.ServerSeq;
            }

            return decrypted;
        }

        private Byte[] P_hash(Byte[] secret, Byte[] seed, Int32 size, IDigest digest)
        {
            var needed = size;

            var md = new HMac(digest);
            var key = new KeyParameter(secret);
            md.Init(key);

            // A_0 is initialized with seed
            var a0 = new Byte[seed.Length];
            Array.Copy(seed, 0, a0, 0, a0.Length);

            // A_i is HMAC of previous A
            var aI = new Byte[md.GetMacSize()];
            md.BlockUpdate(a0, 0, a0.Length);
            md.DoFinal(aI, 0);
            md.Reset();
            var ret = new Byte[needed];

            var outBuff = new Byte[md.GetMacSize()];

            while (needed > 0)
            {
                md.Init(key);
                // Add to return value
                md.BlockUpdate(aI, 0, aI.Length);
                md.BlockUpdate(seed, 0, seed.Length);
                md.DoFinal(outBuff, 0);
                md.Reset();

                var lenToCopy = needed < md.GetMacSize() ? needed : md.GetMacSize();
                Array.Copy(outBuff, 0, ret, size - needed, lenToCopy);

                // Update new A
                md.Init(key);
                md.BlockUpdate(aI, 0, aI.Length);
                md.DoFinal(aI, 0);
                md.Reset();

                // Update needed field
                needed -= md.GetMacSize();
            }

            return ret;
        }

        private Byte[] StreamDecryption(Byte[] source, Byte[] data, HMac hmac, out Int32 sequenceNum)
        {
            // ciphered text consist of data and mac
            Byte[] decrypted;

            if (source.SequenceEqual(this.ClientDirection))
            {
                var d = this.DataDecrypter.DecryptData(data, WriteKeySpec.Client);

                decrypted = new Byte[d.Length];

                Array.Copy(d, decrypted, d.Length);

                var key = new KeyParameter(this.ClientHMacKey);
                hmac.Init(key);

                sequenceNum = this.DataDecrypter.ClientSeq;
            }
            else
            {
                var d = this.DataDecrypter.DecryptData(data, WriteKeySpec.Server);

                decrypted = new Byte[d.Length];

                Array.Copy(d, decrypted, d.Length);

                var key = new KeyParameter(this.ServerHMacKey);
                hmac.Init(key);

                sequenceNum = this.DataDecrypter.ServerSeq;
            }

            return decrypted;
        }

        private Boolean tls_hmac_check(Int32 sequenceNum, HMac hmac, Byte[] ct, Byte[] version, Int32 msgLen,
            Byte[] msg, Byte[] recvDigest)
        {
            var seq = BitConverter.GetBytes(sequenceNum);
            var exSeq = new Byte[8];
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(seq);
            }

            Array.Copy(seq, 0, exSeq, 4, 4);
            hmac.BlockUpdate(exSeq, 0, 8);

            // hash content type
            hmac.BlockUpdate(ct, 0, 1);

            // hash version
            //Array.Reverse(version);
            hmac.BlockUpdate(version, 0, version.Length);

            // hash data length
            var dl = BitConverter.GetBytes(msgLen);
            var dataLength = new Byte[2];
            Array.Copy(dl, 0, dataLength, 0, 2);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(dataLength);
            }

            hmac.BlockUpdate(dataLength, 0, 2);

            // hash data
            //Array.Reverse(msg);
            hmac.BlockUpdate(msg, 0, msgLen);

            // final digest
            var digest = new Byte[hmac.GetMacSize()];
            hmac.DoFinal(digest, 0);
            return digest.SequenceEqual(recvDigest);
        }
    }
}