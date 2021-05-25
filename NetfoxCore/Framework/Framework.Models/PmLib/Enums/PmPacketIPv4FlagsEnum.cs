namespace Netfox.Framework.Models.PmLib.Enums
{
    /// <summary> Gets a IP flags.</summary>
    public enum PmPacketIPv4FlagsEnum
    {
        /// <summary> An enum constant representing the not i pv 4 option.</summary>
        NotIPv4 = -1,

        /// <summary> An enum constant representing the no flags option.</summary>
        NoFlags = 0,

        /// <summary> An enum constant representing the mf option.</summary>
        Mf = 1, //more fragments

        /// <summary> An enum constant representing the df option.</summary>
        Df = 2, //do not fragment

        /// <summary> An enum constant representing the reserved option.</summary>
        Reserved = 4 //must be zero
    }
}