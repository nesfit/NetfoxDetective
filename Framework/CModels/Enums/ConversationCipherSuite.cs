// Copyright (c) 2017 Jan Pluskal
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

namespace Netfox.Framework.Models.Enums
{
    public enum ConversationCipherSuite
    {
        TlsNullWithNullNull = 0x000000,
        TlsRsaWithRc4128Md5 = 0x000004,
        TlsRsaWithRc4128Sha = 0x000005,
        TlsRsaWithIdeaCbcSha = 0x000007,
        TlsRsaWithDesCbcSha = 0x000009,
        TlsRsaWith3DesEdeCbcSha = 0x00000a,
        TlsRsaWithAes128CbcSha = 0x00002f,
        TlsRsaWithAes256CbcSha = 0x000035,
        TlsRsaWithAes128CbcSha256 = 0x00003C,
        TlsRsaWithAes256CbcSha256 = 0x00003D,
        TlsRsaWithCamellia128CbcSha = 0x000041,
        TlsRsaExport1024WithRc456Md5 = 0x000060,
        TlsRsaWithCamellia256CbcSha = 0x000084,
        TlsRsaWithAes128GcmSha256 = 0x00009C,
        TlsRsaWithAes256GcmSha384 = 0x00009D,
        TlsRsaWithCamellia128CbcSha256 = 0x0000BA,
        TlsRsaWithCamellia256CbcSha256 = 0x0000C0,
        TlsRsaWithEstreamSalsa20Sha1 = 0x00E410,
        TlsRsaWithSalsa20Sha1 = 0x00E411
    };
}