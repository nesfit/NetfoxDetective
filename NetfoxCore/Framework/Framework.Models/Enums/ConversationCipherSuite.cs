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
    }
}