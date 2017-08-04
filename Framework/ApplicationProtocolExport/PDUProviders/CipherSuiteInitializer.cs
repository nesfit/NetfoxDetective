// Copyright (c) 2017 Jan Pluskal, Miroslav Slivka, Viliam Letavay
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using Netfox.Framework.Models.Enums;
using Org.BouncyCastle.Crypto.Digests;

namespace Netfox.Framework.ApplicationProtocolExport.PDUProviders
{
    class CipherSuiteInitializer
    {
        public static void PrepareDecryptingAlgorithms(String serverPk, ConversationCipherSuite cipherSuite, out KeyDecrypter keyDecrypter, out DataDecrypter dataDecrypter)
        {
            // TODO For now every key exchange algorithm is either RSA or not supported

            var infoMsg = " CipherSuite not implemented yet.";
            keyDecrypter = new RsaDecrypter(serverPk);

            switch(cipherSuite)
            {
                case ConversationCipherSuite.TlsRsaExport1024WithRc456Md5:
                    throw new NotImplementedException(cipherSuite + infoMsg);
                case ConversationCipherSuite.TlsRsaWith3DesEdeCbcSha:
                    throw new NotImplementedException(cipherSuite + infoMsg);
                case ConversationCipherSuite.TlsRsaWithAes128CbcSha:
                    dataDecrypter = new AesDecrypter(CipherMode.Cbc);
                    dataDecrypter.Digest = new Sha1Digest();
                    dataDecrypter.KeyLength = 128 / 8;
                    dataDecrypter.IvLength = 16;
                    dataDecrypter.MacKeyLength = 20;
                    break;

                case ConversationCipherSuite.TlsRsaWithAes256CbcSha:
                    dataDecrypter = new AesDecrypter(CipherMode.Cbc);
                    dataDecrypter.Digest = new Sha1Digest();
                    dataDecrypter.MacKeyLength = 20;
                    dataDecrypter.KeyLength = 256 / 8;
                    dataDecrypter.IvLength = 16;
                    break;

                case ConversationCipherSuite.TlsRsaWithAes128CbcSha256:
                    dataDecrypter = new AesDecrypter(CipherMode.Cbc);
                    dataDecrypter.Digest = new Sha256Digest();
                    dataDecrypter.KeyLength = 128 / 8;
                    dataDecrypter.IvLength = 16;
                    dataDecrypter.MacKeyLength = 32;
                    break;
                case ConversationCipherSuite.TlsRsaWithAes256CbcSha256:
                    dataDecrypter = new AesDecrypter(CipherMode.Cbc);
                    dataDecrypter.Digest = new Sha256Digest();
                    dataDecrypter.KeyLength = 256 / 8;
                    dataDecrypter.IvLength = 16;
                    dataDecrypter.MacKeyLength = 32;
                    break;

                case ConversationCipherSuite.TlsRsaWithAes128GcmSha256:
                    dataDecrypter = new AesDecrypter(CipherMode.Gcm);
                    dataDecrypter.Digest = new Sha256Digest();
                    dataDecrypter.KeyLength = 128 / 8;
                    dataDecrypter.IvLength = 16;
                    dataDecrypter.MacKeyLength = 32;
                    break;
                case ConversationCipherSuite.TlsRsaWithAes256GcmSha384:
                    dataDecrypter = new AesDecrypter(CipherMode.Gcm);
                    dataDecrypter.Digest = new Sha384Digest();
                    dataDecrypter.KeyLength = 256 / 8;
                    dataDecrypter.IvLength = 16;
                    dataDecrypter.MacKeyLength = 48;
                    break;
                case ConversationCipherSuite.TlsRsaWithCamellia128CbcSha:
                    throw new NotImplementedException(cipherSuite + infoMsg);
                case ConversationCipherSuite.TlsRsaWithCamellia128CbcSha256:
                    throw new NotImplementedException(cipherSuite + infoMsg);
                case ConversationCipherSuite.TlsRsaWithCamellia256CbcSha:
                    throw new NotImplementedException(cipherSuite + infoMsg);
                case ConversationCipherSuite.TlsRsaWithCamellia256CbcSha256:
                    throw new NotImplementedException(cipherSuite + infoMsg);
                case ConversationCipherSuite.TlsRsaWithDesCbcSha:
                    throw new NotImplementedException(cipherSuite + infoMsg);
                case ConversationCipherSuite.TlsRsaWithEstreamSalsa20Sha1:
                    throw new NotImplementedException(cipherSuite + infoMsg);
                case ConversationCipherSuite.TlsRsaWithIdeaCbcSha:
                    throw new NotImplementedException(cipherSuite + infoMsg);
                case ConversationCipherSuite.TlsRsaWithRc4128Md5:
                    dataDecrypter = new Rc4Decrypter(CipherMode.Stream);
                    dataDecrypter.Digest = new MD5Digest();
                    dataDecrypter.KeyLength = 128 / 8;
                    dataDecrypter.IvLength = 0;
                    dataDecrypter.MacKeyLength = 16;
                    break;
                case ConversationCipherSuite.TlsRsaWithRc4128Sha:
                    dataDecrypter = new Rc4Decrypter(CipherMode.Stream);
                    dataDecrypter.Digest = new Sha1Digest();
                    dataDecrypter.KeyLength = 128 / 8;
                    dataDecrypter.IvLength = 0;
                    dataDecrypter.MacKeyLength = 20;
                    break;
                case ConversationCipherSuite.TlsRsaWithSalsa20Sha1:
                    throw new NotImplementedException(cipherSuite + infoMsg);
                default:
                    throw new NotImplementedException(cipherSuite + infoMsg);
            }

            dataDecrypter.ClientSeq = 0;
            dataDecrypter.ServerSeq = 0;
        }
    }
}