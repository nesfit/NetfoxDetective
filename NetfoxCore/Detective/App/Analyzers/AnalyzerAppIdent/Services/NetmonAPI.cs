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

using System;
using System.Runtime.InteropServices;

namespace Netfox.AnalyzerAppIdent.Services
{
    #region Network Monitor API structs and defines

    /// <summary>
    /// Network Monitor API Constants
    /// </summary>
    public class NmConstant
    {
        /// <summary>
        /// 
        /// </summary>
        public const int MAC_ADDRESS_SIZE = 6;
        /// <summary>
        /// 
        /// </summary>
        public const int MAX_PATH = 260;
        /// <summary>
        /// 
        /// </summary>
        public const int NMAPI_GUID_SIZE = 16;
    }

    /// <summary>
    /// Network Monitor API return status code
    /// </summary>
    public class NmStatusCode
    {
        /// <summary>
        /// MessageId: NM_STATUS_FRAME_TOO_BIG_FOR_FILE
        ///
        /// MessageText:
        ///
        /// The file doesn't have enough space to hold this frame.
        /// </summary>
        public const UInt32 NM_STATUS_FRAME_TOO_BIG_FOR_FILE = 0xE1110001;

        /// <summary>
        ///
        /// MessageId: NM_STATUS_FILE_TOO_SMALL
        ///
        /// MessageText:
        ///
        /// Capture file size too small. 
        ///
        /// </summary>
        public const UInt32 NM_STATUS_FILE_TOO_SMALL = 0xE1110002;

        /// <summary>
        ///
        /// MessageId: NM_STATUS_FILE_TOO_LARGE
        ///
        /// MessageText:
        ///
        /// Capture file size too large. 
        ///
        /// </summary>
        public const UInt32 NM_STATUS_FILE_TOO_LARGE = 0xE1110003;

        /// <summary>
        ///
        /// MessageId: NM_STATUS_FRAME_CONTINUES_INTO_NEXT_FRAME
        ///
        /// MessageText:
        ///
        /// The frame is corrupt. It overlaps with the next frame. 
        ///
        /// </summary>
        public const UInt32 NM_STATUS_FRAME_CONTINUES_INTO_NEXT_FRAME = 0xE1110004;

        /// <summary>
        ///
        /// MessageId: NM_STATUS_FRAME_RANGE_OUT_OF_BOUNDS
        ///
        /// MessageText:
        ///
        /// The frame is corrupt. The dimensions of the frame are not in the range of the capture file. 
        ///
        /// </summary>
        public const UInt32 NM_STATUS_FRAME_RANGE_OUT_OF_BOUNDS = 0xE1110005;

        /// <summary>
        ///
        /// MessageId: NM_STATUS_WRONG_ENDIAN
        ///
        /// MessageText:
        ///
        /// The data is in BigEndian and we support only Little Endian 
        ///
        /// </summary>
        public const UInt32 NM_STATUS_WRONG_ENDIAN = 0xE1110006;

        /// <summary>
        ///
        /// MessageId: NM_STATUS_INVALID_PCAP_FILE
        ///
        /// MessageText:
        ///
        /// This file is not a valid PCAP file
        ///
        /// </summary>
        public const UInt32 NM_STATUS_INVALID_PCAP_FILE = 0xE1110007;

        /// <summary>
        ///
        /// MessageId: NM_STATUS_WRONG_PCAP_VERSION
        ///
        /// MessageText:
        ///
        /// This file is not a supported PCAP version 
        ///
        /// </summary>
        public const UInt32 NM_STATUS_WRONG_PCAP_VERSION = 0xE1110008;

        /// <summary>
        ///
        /// MessageId: NM_STATUS_UNSUPPORTED_FILE_TYPE
        ///
        /// MessageText:
        ///
        /// This file type is not supported.
        ///
        /// </summary>
        public const UInt32 NM_STATUS_UNSUPPORTED_FILE_TYPE = 0xE1110009;

        /// <summary>
        ///
        /// MessageId: NM_STATUS_INVALID_NETMON_CAP_FILE
        ///
        /// MessageText:
        ///
        /// This file type is not a valid Network Monitor capture file.
        ///
        /// </summary>
        public const UInt32 NM_STATUS_INVALID_NETMON_CAP_FILE = 0xE111000A;

        /// <summary>
        ///
        /// MessageId: NM_STATUS_UNSUPPORTED_PCAP_DLT
        ///
        /// MessageText:
        ///
        /// This Pcap data link type is not supported.
        ///
        /// </summary>
        public const UInt32 NM_STATUS_UNSUPPORTED_PCAP_DLT = 0xE111000B;

        /// <summary>
        ///
        /// MessageId: NM_STATUS_API_VERSION_MISMATCHED
        ///
        /// MessageText:
        ///
        /// The current NmApi DLL is different from the required version by the application. 
        ///
        /// </summary>
        public const UInt32 NM_STATUS_API_VERSION_MISMATCHED = 0xE111000C;
    }

    ///
    /// <summary><c>NmCaptureMode</c>Network Monitor capture mode</summary> 
    /// <remarks>
    /// </remarks>
    public enum NmCaptureMode : uint 
    {
        /// <summary>
        /// 
        /// </summary>
        LocalOnly,
        /// <summary>
        /// 
        /// </summary>
        Promiscuous
    }

    ///
    /// <summary><c>NmCaptureFileMode</c>Network Monitor capture file expansion mode</summary> 
    /// <remarks>
    /// </remarks>
    public enum NmCaptureFileFlag : uint
    {
        /// <summary>
        /// 
        /// </summary>
        WrapAround,
        /// <summary>
        /// 
        /// </summary>
        Chain,
        /// <summary>
        /// 
        /// </summary>
        LastFlag
    };

    ///
    /// <summary><c>NmCaptureCallbackExitMode</c>Capture callback function exit mode</summary> 
    /// <remarks>
    ///     NmCaptureStopAndDiscard - NmStopCapture/NmPauseCapture returns immediately user's capture callback function will not be called after
    ///                               NmStopCapture/NmPauseCapture returns
    /// </remarks>
    public enum NmCaptureCallbackExitMode : uint
    {
        /// <summary>
        /// 
        /// </summary>
        DiscardRemainFrames = 1,
        /// <summary>
        /// 
        /// </summary>
        ReturnRemainFrames = 2,
    };

    ///
    /// <summary><c>NmAdapterOpState</c>Network Monitor driver adapter operational states</summary> 
    /// <remarks>
    /// </remarks>
    public enum NmAdapterOpState : uint
    {
        /// <summary>
        /// 
        /// </summary>
        None = 0,
        /// <summary>
        /// 
        /// </summary>
        Bound,
        /// <summary>
        /// 
        /// </summary>
        Stopped,
        /// <summary>
        /// 
        /// </summary>
        Capturing,
        /// <summary>
        /// 
        /// </summary>
        Paused
    };


    ///
    /// <summary><c>NmCallbackMsgType</c>Status levels of the call back message</summary> 
    /// <remarks>
    /// </remarks>
    public enum NmCallbackMsgType : uint
    {
        /// <summary>
        /// 
        /// </summary>
        None,
        /// <summary>
        /// 
        /// </summary>
        Error,
        /// <summary>
        /// 
        /// </summary>
        Warning,
        /// <summary>
        /// 
        /// </summary>
        Information,
        /// <summary>
        /// 
        /// </summary>
        Last
    };

    ///
    /// <summary><c>NmNplParserLoadingOption</c>NPL loading option</summary> 
    /// <remarks>
    /// By default the NmLoadNplOptionNone is used.  Only the user specified NPL path(s) are loaded.
    /// If both NmAppendRegisteredNplSets and a NPL path are specified, the resulting NPL parser will include
    /// Both and the specified NPL path(s) are prefixed.
    /// </remarks>
    public enum NmNplParserLoadingOption : uint
    {
        NmLoadNplOptionNone,
        NmAppendRegisteredNplSets
    };

    ///
    /// <summary><c>NmFrameParserOptimizeOption</c>Frame parser optimization options</summary> 
    /// <remarks>
    /// Options used when create frame parser.
    /// </remarks>
    public enum NmFrameParserOptimizeOption : uint
    {
        ///
        /// Create frame parser without optimization according to the added filter
        ///
        ParserOptimizeNone = 0,
        ///
        /// Create frame parser optimized based on added filters, fields and properties
        ///
        ParserOptimizeFull = 1,

        ParserOptimizeLast

    };

    ///
    /// <summary><c>NmFrameParsingOption</c>Frame parser parsing options</summary> 
    /// <remarks>
    /// Options used by NmParseFrame function.
    /// </remarks>
    [Flags]
    public enum NmFrameParsingOption : uint
    {
        /// <summary>
        /// 
        /// </summary>
        None = 0,
        /// <summary>
        /// Provide full path name of the current field if specified
        /// </summary>
        FieldFullNameRequired = 1,
        /// <summary>
        /// Provide the name of the protocol that contains the current field if specified
        /// </summary>
        ContainingProtocolNameRequired = 2,
        /// <summary>
        /// Provide data type name of the fields
        /// </summary>
        DataTypeNameRequired = 4,
        /// <summary>
        /// Use caller specified frame number
        /// </summary>
        UseFrameNumberParameter = 8,
        ///
        /// Provide the display string of the field
        ///
        FieldDisplayStringRequired = 16,
        ///
        /// Provide the frame conversation information
        ///
        FrameConversationInfoRequired = 32,
        /// <summary>
        /// 
        /// </summary>
        ParsingOptionLast

    };


    ///
    /// <summary><c>FrameFragmentationType</c>Fragmentation types returned in parsed frames</summary> 
    /// <remarks>
    /// </remarks>
    public enum NmFrameFragmentationType : uint
    {
        /// <summary></summary>
        None,
        /// <summary></summary>
        Start,
        /// <summary></summary>
        Middle,
        /// <summary></summary>
        End

    }

    ///
    /// <summary><c>NmParsedFieldNames</c>The name string properties in parsed field</summary> 
    /// <remarks>
    /// </remarks>
    public enum NmParsedFieldNames : uint
    {
        /// <summary></summary>
        NamePath,
        /// <summary></summary>
        DataTypeName,
        /// <summary></summary>
        ContainingProtocolName,
        /// <summary></summary>
        FieldDisplayString
    };

    ///
    /// <summary><c>NmMvsKeyType</c>Key types of the multi storage property</summary> 
    /// <remarks>
    /// The MvsKeyTypeArrayIndex type is used for group property functions to specify the index parameter.
    /// </remarks>
    public enum NmMvsKeyType : uint
    {
        /// <summary></summary>
        MvsKeyTypeNone,
        /// <summary></summary>
        MvsKeyTypeNumber,
        /// <summary></summary>
        MvsKeyTypeString,
        /// <summary></summary>
        MvsKeyTypeByteArray,
        /// <summary></summary>
        MvsKeyTypeArrayIndex,
        /// <summary></summary>
        MvsKeyTypeLast

    };

    ///
    /// <summary>
    /// <c>NmPropertyScope</c>
    /// Scopes of properties.  It is reported in the property info.
    /// </summary> 
    /// <remarks>
    /// </remarks>
    ///
    public enum NmPropertyScope : uint
    {
        /// <summary></summary>
        PropertyScopeNone = 0,
        /// <summary></summary>
        PropertyScopeConversation = 1,
        /// <summary></summary>
        PropertyScopeGlobal = 2,
        /// <summary></summary>
        PropertyScopeFrame = 4

    };

    ///
    /// <summary>
    /// <c>NmPropertyContainerType</c>
    /// The property aggregation form, i.e., MVS with key, Array with index, etc.
    /// </summary> 
    /// <remarks>
    /// </remarks>
    ///
    public enum NmPropertyContainerType : uint
    {
        /// <summary></summary>
        PropertyTypeContainerNone = 0,
        /// <summary></summary>
        PropertyTypeContainerValue,
        /// <summary></summary>
        PropertyTypeContainerMvs,
        /// <summary></summary>
        PropertyTypeContainerArray

    };

    ///
    /// <summary>
    /// <c>NmPropertyValueType</c>
    /// Type of the property value.
    /// </summary> 
    /// <remarks>
    /// Number value is in signed or unsigned integer format
    /// String value is in wide char format
    /// Byte Blob is in byte array
    /// 
    /// The value type of properties, in the same multi value storage property addressed 
    /// By different keys or in the same property group by different indexes,
    /// Can be different.
    /// .
    /// </remarks>
    ///
    public enum NmPropertyValueType : uint
    {
        /// <summary></summary>
        PropertyValueNone,
        /// <summary></summary>
        PropertyValueSignedNumber,
        /// <summary></summary>
        PropertyValueUnsignedNumber,
        /// <summary></summary>
        PropertyValueString,
        /// <summary></summary>
        PropertyValueByteBlob

    };

    ///
    /// <summary><c>NmOlpActionFlags</c></summary> 
    /// <remarks>
    /// These flags are used when the OLP expressions, blocks and filters are created.  For filter creation, these
    /// flags are combined with NmFilterOptionFlags in the options parameter.  These flags override the 
    /// normal AND/OR operations that would occur for the conditions, blocks and filters following the current
    /// entity; OLP expression, block or filter.
    ///
    /// For example, if block type is AND and the NmOlpActionFlagsCopyOnFalse is specified at block creation, 
    /// the frames that fail the block are copied to user mode although they would have been dropped in the 
    /// normal AND operation.  These flags "short circuit" the rest of the evaluation.  If this evaluation is 
    /// true then the driver continues to evaluate the remaining blocks if they exist, or copies this frame to
    /// user mode if this is the only block.  This behavior is identical for all three levels: condition, block 
    /// and filter.
    ///
    /// The action flags are exclusive, i.e., they cannot be set at the same time that could be done by mistake 
    /// In NmCreateOlpFilter where the two flags are combined.
    /// </remarks>
    public enum NmOlpActionFlags : uint
    {
        /// <summary></summary>
        NmOlpActionFlagsNone,
        /// <summary></summary>
        NmOlpActionFlagsCopyOnFalse,
        /// <summary></summary>
        NmOlpActionFlagsDropOnFalse,
        /// <summary></summary>
        NmOlpActionFlagsLast

    };

    ///
    /// <summary><c>NmFilterOptionFlags</c></summary> 
    /// <remarks>
    /// Filter options are used for filter creation only.
    ///
    /// NmFilterOptionFlagsBlockTypeAnd specifies the logical operation among all blocks contained in the filter 
    /// and the opposite logical operation is performed among all the OLP conditions in each block.
    /// 
    /// NmFilterOptionFlagsAndToNext specifies the logical operation between this filter and the next. Note that 
    /// multiple filters are evaluated in the order that they are added.  If filter A is added first followed by 
    /// filter B, A is evaluated first.  By default, when this flag is not set, A is OR'd with B.
    /// 
    /// A is AND'd with B if NmFilterOptionFlagsAndToNext is set in filter A during creation.  The frames are dropped 
    /// if A fails; B will be evaluated if A passes.
    ///
    /// If there are filters A, B and C, and A has NmFilterOptionFlagsAndToNext set, the logic is A AND B OR C, i.e., it 
    /// Only affect the operation between the two adjacent filters, given they are added in order of A, B and C.
    ///
    /// NmFilterOptionFlagsBlockTypeAnd and NmFilterOptionFlagsAndToNext are not exclusive to each other.  They can 
    /// also be combined with NmOlpActionFlags during filter creation.  NmOlpActionFlags override NmFilterOptionFlags
    /// when the action criteria is met.  The NmOlpActionFlags are in the lower WORD of the combined flags.
    ///
    /// </remarks>
    public enum NmFilterOptionFlags : uint
    {
        /// <summary></summary>
        NmFilterOptionFlagsNone,
        /// <summary>
        /// If set, the blocks contained in the filter are AND together.
        /// </summary>
        NmFilterOptionFlagsBlockTypeAnd = 0x00010000,
        /// <summary>
        /// If set, the current filter is AND to the next filter if exist; otherwise 
        /// It is OR to the next filter
        /// </summary>
        NmFilterOptionFlagsAndToNext = 0x00020000,
        /// <summary></summary>
        NmFilterOptionFlagsLast

    };

    ///
    /// <summary><c>NmFilterMatchMode</c>Filter match mode for TRUE</summary> 
    /// <remarks>
    /// NmFilterMatchModeEqual:     return TRUE if pattern matches the frame data.
    /// NmFilterMatchModeNotEqual:  return TRUE if pattern does not match the frame data.
    /// NmFilterMatchModeGreater:   return TRUE if pattern is greater than the frame data.
    /// NmFilterMatchModeLesser:    return TRUE if pattern is less than the frame data.
    /// </remarks>
    public enum NmFilterMatchMode : uint
    {
        /// <summary></summary>
        NmFilterMatchModeNone,
        /// <summary></summary>
        NmFilterMatchModeEqual,
        /// <summary></summary>
        NmFilterMatchModeNotEqual,
        /// <summary></summary>
        NmFilterMatchModeGreater,
        /// <summary></summary>
        NmFilterMatchModeLesser,
        /// <summary></summary>
        NmFilterMatchModeLast

    };

    ///
    /// <summary><c>NM_CAPTURE_STATISTICS</c></summary> 
    /// <remarks>
    /// The statistics is per adapter in API.  To get that for the whole engine, caller can
    /// Sum up all capturing adapters.
    /// 
    /// The EngineDropCount is drop count caused by capture engine in user mode for each adapter.
    /// </remarks>
    public struct NM_CAPTURE_STATISTICS
    {
        /// <summary>
        /// For version control
        /// </summary>
        public System.UInt16 Size;
        /// <summary>
        /// The frame drop count in driver
        /// </summary>
        public System.UInt64 DriverDropCount;
        /// <summary>
        /// The count of frames that are filtered by driver.
        /// </summary>
        public System.UInt64 DriverFilteredCount;
        /// <summary>
        /// The count of frames that have be seen in driver.
        /// </summary>
        public System.UInt64 DriverSeenCount;
        /// <summary>
        /// The frame drop count in capture engine.
        /// </summary>
        public System.UInt64 EngineDropCount;

    };

    ///
    /// <summary><c>CNmPropertyStorageKey</c></summary> 
    /// <remarks>
    /// The top three fields are marshaled to native code.
    /// Create key using the provided methods according to the key types 
    /// (number, string or byte array.)
    /// </remarks>
    /// <example>
    /// <code>
    /// CNmPropertyStorageKey [] keyArray = new CNmPropertyStorageKey[3];
    /// CNmPropertyStorageKey key0 = new CNmPropertyStorageKey();
    /// CNmPropertyStorageKey key1 = new CNmPropertyStorageKey();
    /// CNmPropertyStorageKey key2 = new CNmPropertyStorageKey();
    /// NmPropertyValueType vType;
    /// int myInt;
    /// // Build a Number key
    /// key0.SetNumberKey(0x9a);
    /// // Build a string key
    /// key1.SetStringKey("MyKey");
    /// // build a blob key
    /// byte [] keyBuffer = new byte[16];
    /// for (int i = 0; i < 16; ++i)
    /// {
    ///    keyBuffer[i] = (byte)i;
    /// }
    /// key2.SetByteArrayKey(keyBuffer, 16);
    /// keyArray[0] = key0;
    /// keyArray[1] = key1;
    /// keyArray[2] = key2;
    /// </code>
    /// </example>
    public struct CNmPropertyStorageKey
    {
        public NmMvsKeyType KeyType;
        public int Length;
        public System.IntPtr ValuePointer;

        /// <summary>
        /// For multi-value storage key in string form
        /// </summary>
        public void SetStringKey(String KeyValue)
        {
            if (null != KeyValue)
            {
                this.KeyType = NmMvsKeyType.MvsKeyTypeString;
                this.ValuePointer = Marshal.StringToHGlobalUni(KeyValue);
                this.Length = KeyValue.Length;
            }
            else
            {
                throw new System.ArgumentNullException("NULL String pointer");
            }
        }

        /// <summary>
        /// For multi-value storage key in numeric form (64-bit)
        /// </summary>
        public void SetNumberKey(Int64 KeyValue)
        {
            this.KeyType = NmMvsKeyType.MvsKeyTypeNumber;
            this.ValuePointer = Marshal.AllocHGlobal(8);
            Marshal.WriteInt64(this.ValuePointer, KeyValue);
            this.Length = 8;
        }

        /// <summary>
        /// For property in Array
        /// </summary>
        public void SetIndexKey(Int64 KeyValue)
        {
            this.KeyType = NmMvsKeyType.MvsKeyTypeArrayIndex;
            this.ValuePointer = Marshal.AllocHGlobal(8);
            Marshal.WriteInt64(this.ValuePointer, KeyValue);
            this.Length = 8;
        }

        /// <summary>
        /// For multi-value storage key in byte array form
        /// </summary>
        public void SetByteArrayKey(byte[] KeyValue, Int32 KeyLength)
        {
            if (null != KeyValue)
            {
                this.KeyType = NmMvsKeyType.MvsKeyTypeByteArray;
                this.ValuePointer = Marshal.AllocHGlobal(KeyLength);
                for (int i = 0; i < KeyLength; ++i)
                {
                    Marshal.WriteByte(this.ValuePointer, i, KeyValue[i]);
                }
                this.Length = KeyLength;
            }
            else
            {
                throw new System.ArgumentNullException("NULL KeyValue Byte Array.");
            }
        }
    };

    ///
    /// <summary><c>NM_NPL_PROPERTY_INFO</c></summary> 
    /// <remarks>
    /// Contains runtime information for instantiated properties
    /// </remarks>
    ///
    public struct NM_NPL_PROPERTY_INFO
    {
        /// <summary>
        /// For version control
        /// </summary>
        public System.UInt16 Size;
        /// <summary>
        /// Property Scope
        /// </summary>
        public NmPropertyScope Scope;
        /// <summary>
        /// Property container type, e.g., MVS, Array.
        /// </summary>
        public NmPropertyContainerType ContainerType;
        /// <summary>
        /// The element count of the name excluding the terminator
        /// </summary>
        public System.UInt16 NameSize;
        /// <summary>
        /// Property string added by NmAddProperty
        /// </summary>
        [MarshalAs(UnmanagedType.LPWStr)]
        public System.String Name;
        /// <summary>
        /// The data type of the property
        /// </summary>
        public NmPropertyValueType ValueType;
        /// <summary>
        /// The size of the value.  If the value type is string, the terminator is not included.
        /// </summary>
        public System.UInt32 ValueSize;
        /// <summary>
        /// number of items in Array.
        /// </summary>
        public System.UInt32 ItemCount;

    };

    ///
    /// <summary><c>NmFragmentationInfo</c>Fragmentation information returned in parsed frames</summary> 
    /// <remarks>
    /// </remarks>
    public struct NmFragmentationInfo
    {
        /// <summary>
        /// </summary>
        public UInt16 Size;

        /// <summary>
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U2, SizeConst = NmConstant.MAX_PATH)]
        public System.Char[] FragmentedProtocolName;

        /// <summary>
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U2, SizeConst = NmConstant.MAX_PATH)]
        public System.Char[] PayloadProtocolName;

        /// <summary>
        /// </summary>
        public NmFrameFragmentationType FragmentType;
    };

    /// <summary>
    /// Callback function for frame receiving/retrieving
    /// </summary>
    public delegate void CaptureCallbackDelegate(IntPtr hCaptureEngine,
                                                 UInt32 ulAdapterIndex,
                                                 IntPtr pCallerContext,
                                                 IntPtr hFrame);



    ///
    /// <summary><c>NmConversationOption</c>Frame parser conversation configuration options</summary> 
    /// <remarks>
    /// </remarks>
    public enum NmConversationConfigOption : uint
    {
        /// <summary>
        /// </summary>
        None,
        /// <summary>
        /// </summary>
        Last
    };

    ///
    /// <summary><c>NmReassemblyOption</c>Frame parser reassembly configuration options</summary> 
    /// <remarks>
    /// </remarks>
    public enum NmReassemblyConfigOption : uint
    {
        /// <summary>
        /// </summary>
        None,
        /// <summary>
        /// </summary>
        Last
    };

    /// <summary>
    /// Callback function for parser compile/build process.
    /// </summary>
    public delegate void ParserCallbackDelegate(IntPtr pCallerContext,
                                                UInt32 ulStatusCode,
                                                [MarshalAs(UnmanagedType.LPWStr)] String lpDescription,
                                                NmCallbackMsgType ulType);

    /// <summary>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NM_NIC_ADAPTER_INFO
    {
        /// <summary>
        /// </summary>
        public System.UInt16 Size;

        /// <summary>
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = NmConstant.MAC_ADDRESS_SIZE)]
        public System.Byte[] PermanentAddr;
        /// <summary>
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = NmConstant.MAC_ADDRESS_SIZE)]
        public System.Byte[] CurrentAddr;

        /// <summary>
        /// </summary>
        public NDIS_MEDIUM MediaType;

        /// <summary>
        /// </summary>
        public NDIS_PHYSICAL_MEDIUM PhysicalMediaType;

        /// <summary>
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U2, SizeConst = NmConstant.MAX_PATH)]
        public System.Char[] ConnectionName;
        /// <summary>
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U2, SizeConst = NmConstant.MAX_PATH)]
        public System.Char[] FriendlyName;
        /// <summary>
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U2, SizeConst = NmConstant.MAX_PATH)]
        public System.Char[] Guid;

        ///
        /// Network adapter operational state. Indicates if the network adapter is bound, capturing, pause or stopped
        ///
        public NmAdapterOpState OpState;
        ///
        /// Indicates if the network adapter is enabled or disabled. It only can be enabled if it is bound to the Network Monitor driver
        ///
        public System.Int32 Enabled;

        /// <summary>
        /// </summary>
        public System.Int32 PModeEnabled;

        ///
        /// Frame indication callback is assigned by adapter
        ///
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public CaptureCallbackDelegate CallBackFunction;
    };

    /// <summary>
    ///
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NM_API_CONFIGURATION
    {
        ///
        /// Configurable limits that overwrite default API settings 
        ///
        public System.UInt16 Size;
        /// <summary>
        /// </summary>
        public System.UInt32 RawFrameHandleCountLimit;
        /// <summary>
        /// </summary>
        public System.UInt32 ParsedFrameHandleCountLimit;
        /// <summary>
        /// </summary>
        public System.UInt32 CaptureEngineCountLimit;
        /// <summary>
        /// </summary>
        public System.UInt32 NplParserCountLimit;
        /// <summary>
        /// </summary>
        public System.UInt32 FrameParserConfigCountLimit;
        /// <summary>
        /// </summary>
        public System.UInt32 FrameParserCountLimit;
        /// <summary>
        /// </summary>
        public System.UInt32 CaptureFileCountLimit;

        ///
        /// API threading mode for COM initialization.  
        /// Only support COINIT_MULTITHREADED and COINIT_APARTMENTTHREADED
        ///
        public System.UInt16 ThreadingMode;

        ///
        /// Configurable default feature/behavior parameters
        ///
        public NmConversationConfigOption ConversationOption;
        /// <summary>
        /// </summary>
        public NmReassemblyConfigOption ReassemblyOption;
        /// <summary>
        /// </summary>
        public NmCaptureFileFlag CaptureFileMode;
        /// <summary>
        /// </summary>
        public NmFrameParsingOption FrameParsingOption;
        /// <summary>
        /// </summary>
        public NmCaptureCallbackExitMode CaptureCallbackExitMode;

        ///
        /// Hard limits the API enforce (not configurable)
        ///
        public System.UInt32 MaxCaptureFileSize;
        /// <summary>
        /// </summary>
        public System.UInt32 MinCaptureFileSize;

        /// Maximum number of handles per handle type  
        public System.UInt32 MaxApiHandleLimit;
    };

    ///
    /// <summary><c>NM_PROTOCOL_SEQUENCE_CONFIG</c>Data structure for API user to specify NPL properties and fields
    ///                                            For frame order correction support.
    /// </summary> 
    /// <remarks>
    /// </remarks>
    ///
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NM_PROTOCOL_SEQUENCE_CONFIG
    {
        ///
        /// API verifies the member 'Size' against the size of its version.  They must match.
        ///
        public UInt16 Size;

        ///
        /// The names of the properties containing the values to form the key 
        /// to identify the group of the frames to get in order.  If multiple names are used,
        /// They are separated by semicolons.  The string must be NULL terminated.
        ///
        [MarshalAs(UnmanagedType.LPWStr)]
        public String GroupKeyString;

        ///
        /// The name of the property containing the frame's sequence number.
        ///
        [MarshalAs(UnmanagedType.LPWStr)]
        public String SequencePropertyString;

        ///
        /// The name of the property containing the frame's next sequence number.
        ///
        [MarshalAs(UnmanagedType.LPWStr)]
        public String NextSequencePropertyString;

    };

    ///
    /// <summary><c>NM_ORDER_PARSER_PARAMETER</c>Data structure for calling NmOpCaptureFileInOrder</summary>
    /// 
    /// <remarks>
    /// </remarks>
    ///
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NM_ORDER_PARSER_PARAMETER
    {
        ///
        /// API verifies the member 'Size' against the size of its version.  They must match.
        ///
        public UInt16 Size;

        ///
        /// The frame parser used for handling out of order frames.  It must be built from a frame parser
        /// Configuration that has sequence information specified by NM_PROTOCOL_SEQUENCE_CONFIG.
        ///
        public IntPtr hFrameParser;

        ///
        /// For future option flags.
        ///
        public UInt32 Option;

    };

    /// <summary>
    /// </summary>
    public enum NDIS_MEDIUM
    {
        /// <summary>
        /// </summary>
        Ndis_802_3,
        /// <summary>
        /// </summary>
        Ndis_802_5,
        /// <summary>
        /// </summary>
        Ndis_Fddi,
        /// <summary>
        /// </summary>
        Ndis_Wan,
        /// <summary>
        /// </summary>
        Ndis_LocalTalk,
        /// <summary>
        /// </summary>
        Ndis_Dix,              // defined for convenience, not a real medium
        /// <summary>
        /// </summary>
        Ndis_ArcnetRaw,
        /// <summary>
        /// </summary>
        Ndis_Arcnet878_2,
        /// <summary>
        /// </summary>
        Ndis_Atm,
        /// <summary>
        /// </summary>
        Ndis_WirelessWan,
        /// <summary>
        /// </summary>
        Ndis_Irda,
        /// <summary>
        /// </summary>
        Ndis_Bpc,
        /// <summary>
        /// </summary>
        Ndis_CoWan,
        /// <summary>
        /// </summary>
        Ndis_1394,
        /// <summary>
        /// </summary>
        Ndis_InfiniBand,
        /// <summary>
        /// #if ((NTDDI_VERSION >= NTDDI_VISTA) || NDIS_SUPPORT_NDIS6)
        /// </summary>
        Ndis_Tunnel,
        /// <summary>
        /// </summary>
        Ndis_Native802_11,
        /// <summary>
        /// </summary>
        Ndis_Loopback,
        /// <summary>
        /// #endif // (NTDDI_VERSION >= NTDDI_VISTA)
        /// </summary>
        NdisMediumMax               // Not a real medium, defined as an upper-bound
    };


    /// <summary>
    /// </summary>
    public enum NDIS_PHYSICAL_MEDIUM
    {
        /// <summary>
        /// </summary>
        Ndis_Unspecified,
        /// <summary>
        /// </summary>
        Ndis_WirelessLan,
        /// <summary>
        /// </summary>
        Ndis_CableModem,
        /// <summary>
        /// </summary>
        Ndis_PhoneLine,
        /// <summary>
        /// </summary>
        Ndis_PowerLine,
        /// <summary>
        /// includes ADSL and UADSL (G.Lite)
        /// </summary>
        Ndis_DSL,
        /// <summary>
        /// </summary>
        Ndis_FibreChannel,
        /// <summary>
        /// </summary>
        Ndis_1394,
        /// <summary>
        /// </summary>
        Ndis_WirelessWan,
        /// <summary>
        /// </summary>
        Ndis_Native802_11,
        /// <summary>
        /// </summary>
        Ndis_Bluetooth,
        /// <summary>
        /// </summary>
        Ndis_Infiniband,
        /// <summary>
        /// </summary>
        Ndis_WiMax,
        /// <summary>
        /// </summary>
        Ndis_UWB,
        /// <summary>
        /// </summary>
        Ndis_802_3,
        /// <summary>
        /// </summary>
        Ndis_802_5,
        /// <summary>
        /// </summary>
        Ndis_Irda,
        /// <summary>
        /// </summary>
        Ndis_WiredWAN,
        /// <summary>
        /// </summary>
        Ndis_WiredCoWan,
        /// <summary>
        /// </summary>
        Ndis_Other,
        /// <summary>
        /// </summary>
        NdisPhysicalMediumMax       // Not a real physical type, defined as an upper-bound
    };

    /// <summary>
    /// http://www.marin.clara.net/COM/variant_type_definitions.htm
    /// </summary>
    public class FieldType
    {
        /// <summary>
        /// </summary>
        public const UInt16 VT_EMPTY = 0;
        /// <summary>
        /// </summary>
        public const UInt16 VT_NULL = 1;
        /// <summary>
        /// </summary>
        public const UInt16 VT_I2 = 2;
        /// <summary>
        /// </summary>
        public const UInt16 VT_I4 = 3;
        /// <summary>
        /// </summary>
        public const UInt16 VT_R4 = 4;
        /// <summary>
        /// </summary>
        public const UInt16 VT_R8 = 5;
        /// <summary>
        /// </summary>
        public const UInt16 VT_CY = 6;
        /// <summary>
        /// </summary>
        public const UInt16 VT_DATE = 7;
        /// <summary>
        /// </summary>
        public const UInt16 VT_BSTR = 8;
        /// <summary>
        /// </summary>
        public const UInt16 VT_DISPATCH = 9;
        /// <summary>
        /// </summary>
        public const UInt16 VT_ERROR = 10;
        /// <summary>
        /// </summary>
        public const UInt16 VT_BOOL = 11;
        /// <summary>
        /// </summary>
        public const UInt16 VT_VARIANT = 12;
        /// <summary>
        /// </summary>
        /// <summary>
        /// </summary>
        public const UInt16 VT_UNKNOWN = 13;
        /// <summary>
        /// </summary>
        public const UInt16 VT_DECIMAL = 14;
        /// <summary>
        /// </summary>
        public const UInt16 VT_I1 = 16;
        /// <summary>
        /// </summary>
        public const UInt16 VT_UI1 = 17;
        /// <summary>
        /// </summary>
        public const UInt16 VT_UI2 = 18;
        /// <summary>
        /// </summary>
        public const UInt16 VT_UI4 = 19;
        /// <summary>
        /// </summary>
        public const UInt16 VT_I8 = 20;
        /// <summary>
        /// </summary>
        public const UInt16 VT_UI8 = 21;
        /// <summary>
        /// </summary>
        public const UInt16 VT_INT = 22;
        /// <summary>
        /// </summary>
        public const UInt16 VT_UINT = 23;
        /// <summary>
        /// </summary>
        public const UInt16 VT_VOID = 24;

        /// <summary>
        /// </summary>
        public const UInt16 VT_HRESULT = 25;
        /// <summary>
        /// </summary>
        public const UInt16 VT_PTR = 26;
        /// <summary>
        /// </summary>
        public const UInt16 VT_SAFEARRAY = 27;

        /// <summary>
        /// </summary>
        public const UInt16 VT_CARRAY = 28;
        /// <summary>
        /// </summary>
        public const UInt16 VT_USERDEFINED = 29;
        /// <summary>
        /// </summary>
        public const UInt16 VT_LPSTR = 30;

        /// <summary>
        /// </summary>
        public const UInt16 VT_LPWSTR = 31;
        /// <summary>
        /// </summary>
        public const UInt16 VT_FILETIME = 64;
        /// <summary>
        /// </summary>
        public const UInt16 VT_BLOB = 65;

        /// <summary>
        /// </summary>
        public const UInt16 VT_STREAM = 66;
        /// <summary>
        /// </summary>
        public const UInt16 VT_STORAGE = 67;
        /// <summary>
        /// </summary>
        public const UInt16 VT_STREAMED_OBJECT = 68;

        /// <summary>
        /// </summary>
        public const UInt16 VT_STORED_OBJECT = 69;
        /// <summary>
        /// </summary>
        public const UInt16 VT_BLOB_OBJECT = 70;
        /// <summary>
        /// </summary>
        public const UInt16 VT_CF = 71;

        /// <summary>
        /// </summary>
        public const UInt16 VT_CLSID = 72;
        /// <summary>
        /// </summary>
        public const UInt16 VT_VECTOR = 0x1000;
        /// <summary>
        /// </summary>
        public const UInt16 VT_ARRAY = 0x2000;

        /// <summary>
        /// </summary>
        public const UInt16 VT_BYREF = 0x4000;
        /// <summary>
        /// </summary>
        public const UInt16 VT_RESERVED = 0x8000;
        /// <summary>
        /// </summary>
        public const UInt16 VT_ILLEGAL = 0xffff;

        /// <summary>
        /// </summary>
        public const UInt16 VT_ILLEGALMASKED = 0xfff;
        /// <summary>
        /// </summary>
        public const UInt16 VT_TYPEMASK = 0xfff;
    }

    ///
    /// Returned to caller from NmGetParsedFieldInfo function
    ///     API set the member 'Size' when return for struct version checking purpose
    ///     Member 'NplDataTypeNameLength' and 'ProtocolNameLength' are 0 if not requested by caller when 
    ///     Invoke parsing function.  All string length parameters are in element, e.g., in Unicode here.
    ///
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NmParsedFieldInfo
    {
        ///
        /// API set the member 'Size' when return for struct version checking purpose
        ///
        public System.UInt16 Size;
        ///
        /// The relative level to the root protocol
        ///
        public System.UInt16 FieldIndent;
        ///
        /// The size of the string that holds the full path of the data field if the NmFrameParseOptions 
        /// FieldFullNameRequired is set, e.g., Frame.Ethernet.IPv4.SourceAddress;  Otherwise it is the size
        /// of the current field name only
        ///
        public System.UInt16 NamePathLength;
        ///
        /// The size of the string that contains the name of the NPL data type if the NmFrameParseOptions 
        /// DataTypeNameRequired is set, e.g., "UINT16";  Otherwise it is zero.
        ///
        public System.UInt16 NplDataTypeNameLength;
        ///
        /// The size of the string tht contains the protocol containing the field if the NmFrameParseOptions 
        /// ContainingProtocolNameRequired is set;  Otherwise it is zero
        ///
        public System.UInt16 ProtocolNameLength;
        ///
        /// The size of the display string of the field if the NmFrameParseOptions 
        /// FieldDisplayStringRequired is set;  Otherwise it is zero
        ///
        public System.UInt16 DisplayStringLength;
        ///
        /// Offset in current protocol
        ///
        public System.UInt32 ProtocolBitOffset;
        ///
        /// Field offset in frame
        ///
        public System.UInt32 FrameBitOffset;
        ///
        /// Length of the field
        ///
        public System.UInt32 FieldBitLength;
        ///
        /// The variant type defined as in VARENUM
        ///
        public System.UInt16 ValueType;
        ///
        /// The size of the buffer required to hold the field value represented in VARIANT struct including
        /// The length of the content if the VARIANT contains a pointer to ARRAY or string.
        ///
        public System.UInt16 ValueBufferLength;
    };

    ///
    /// Returned to caller from NmGetParsedFieldInfoEx function
    ///     API set the member 'Size' when return for struct version checking purpose
    ///     Member 'NplDataTypeNameLength' and 'ProtocolNameLength' are 0 if not requested by caller when 
    ///     Invoke parsing function.  All string length parameters are in element, e.g., in Unicode here.
    ///
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NmParsedFieldInfoEx
    {
        ///
        /// API set the member 'Size' when return for struct version checking purpose
        ///
        public System.UInt16 Size;
        ///
        /// The relative level to the root protocol
        ///
        public System.UInt16 FieldIndent;
        ///
        /// The size of the string that holds the full path of the data field if the NmFrameParseOptions 
        /// FieldFullNameRequired is set, e.g., Frame.Ethernet.IPv4.SourceAddress;  Otherwise it is the size
        /// of the current field name only
        ///
        public System.UInt16 NamePathLength;
        ///
        /// The size of the string that contains the name of the NPL data type if the NmFrameParseOptions 
        /// DataTypeNameRequired is set, e.g., "UINT16";  Otherwise it is zero.
        ///
        public System.UInt16 NplDataTypeNameLength;
        ///
        /// The size of the string tht contains the protocol containing the field if the NmFrameParseOptions 
        /// ContainingProtocolNameRequired is set;  Otherwise it is zero
        ///
        public System.UInt16 ProtocolNameLength;
        ///
        /// The size of the display string of the field if the NmFrameParseOptions 
        /// FieldDisplayStringRequired is set;  Otherwise it is zero
        ///
        public System.UInt16 DisplayStringLength;
        ///
        /// Offset in current protocol
        ///
        public System.UInt32 ProtocolBitOffset;
        ///
        /// Field offset in frame
        ///
        public System.UInt32 FrameBitOffset;
        ///
        /// Length of the field
        ///
        public System.UInt32 FieldBitLength;
        ///
        /// The variant type defined as in VARENUM
        ///
        public System.UInt16 ValueType;
        ///
        /// The size of the buffer required to hold the field value represented in VARIANT struct including
        /// The length of the content if the VARIANT contains a pointer to ARRAY or string.
        ///
        public System.UInt32 ValueBufferLength;
    };

    ///
    /// <summary><c>NmSystemTime</c>SYSTEMTIME structure in C++</summary>
    /// 
    /// <remarks>
    /// </remarks>
    ///
    [StructLayout(LayoutKind.Explicit, Size = 16, CharSet = CharSet.Ansi)]
    public class NmSystemTime
    {
        [FieldOffset(0)]
        public ushort wYear;
        [FieldOffset(2)]
        public ushort wMonth;
        [FieldOffset(4)]
        public ushort wDayOfWeek;
        [FieldOffset(6)]
        public ushort wDay;
        [FieldOffset(8)]
        public ushort wHour;
        [FieldOffset(10)]
        public ushort wMinute;
        [FieldOffset(12)]
        public ushort wSecond;
        [FieldOffset(14)]
        public ushort wMilliseconds;
    }

    ///
    /// <summary><c>TIME_ZONE_INFORMATION</c>Same data structure in C++</summary>
    /// 
    /// <remarks>
    /// </remarks>
    ///
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct TIME_ZONE_INFORMATION
    {
        [MarshalAs(UnmanagedType.I4)]
        public int Bias;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
        public string StandardName;
        public NmSystemTime StandardDate;
        [MarshalAs(UnmanagedType.I4)]
        public int StandardBias;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
        public string DaylightName;
        public NmSystemTime DaylightDate;
        [MarshalAs(UnmanagedType.I4)]
        public int DaylightBias;
    }


    ///
    /// <summary><c>NM_TIME</c>Data structure for NmGetFrameTimeStampEx and NmBuildRawFrameFromBufferEx function</summary>
    /// 
    /// <remarks>
    /// </remarks>
    ///
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct NM_TIME
    {
        ///
        /// Size of structure that is set by the caller.
        ///
        public System.Int32 NmTimeStampSize;
        ///
        /// UTC Time Stamp.
        ///
        public System.Int64 TimeStamp;
        ///
        /// TRUE if the time is originally UTC. FALSE if the time was local and converted to UTC.
        ///
        public System.Boolean IsUTC;
        ///
        /// TRUE if the time zone information exists.
        ///
        public System.Boolean HasTZI;
        ///
        /// Time zone information for the frame.
        /// If TRUE == isUTC and TRUE == hasTZI, TZI contains correct time zone information
        /// If TRUE == isUTC and FALSE == hasTZI, TZI does not contain time zone information
        /// if FALSE == isUTC and FALSE == hasTZI, Time stamp is originally local time and converted to UTC using LocalFileTimeToFileTime(). TZI does not contain time zone information.
        /// If FALSE == isUTC and TRUE == hasTZI, At the moment, there is no scenario to be this case.
        ///
        public TIME_ZONE_INFORMATION TZI;
    };

    /// <summary><c>NmNplProfileAttribute</c></summary> 
    /// <remarks>
    /// The NmNplProfileAttribute enumeration is used to select which string the profile should return when
    /// using the SetNplProfileAttribute and GetNplProfileAttribute methods.
    /// </remarks>
    public enum NmNplProfileAttribute
    {
        NmNplProfileAttributeName,
        NmNplProfileAttributeGuid,
        NmNplProfileAttributeDescription,
        NmNplProfileAttributeIncludePath,
        NmNplProfileAttributeDirectory,
        NmNplProfileAttributePackageName,
        NmNplProfileAttributePackageVersion,
        NmNplProfileAttributePackageGuid,
        NmNplProfileAttributeDependencies,
        NmNplProfileAttributeTypeDescription,
    };



    #endregion

    /// <summary>
    /// PInvoke wrapper of Network Monitor API.
    ///     - managed/unmanaged data type mapping: http://msdn2.microsoft.com/en-us/library/ac7ay120.aspx
    ///     - the declaration in this class are strongly tied to nmapi.h
    /// </summary>
    public class NetmonAPI
    {
        #region API Operations

        /// <summary><c>NmGetApiVersion</c>Query current version</summary> 
        /// <remarks>
        /// The API version matches Network Monitor engine version.
        /// </remarks>
        /// <example>
        /// <code>
        ///     UInt16 majorNumber = 0;
        ///     UInt16 minorNumber = 0;
        ///     UInt16 BuildNumber = 0;
        ///     UInt16 RevisionNumber = 0;
        ///     NmGetApiVersion(out majorNumber, out minorNumber, out BuildNumber, out RevisionNumber);
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="Major">[out] Major version number</param>
        /// <param name="Minor">[out] Minor version number</param>
        /// <param name="Build">[out] Build number</param>
        /// <param name="Revision">[out] Revision number</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>Return nothing</returns>
        [DllImport("NmApi.Dll")]
        public static extern void NmGetApiVersion(out UInt16 Major, out UInt16 Minor, out UInt16 Build, out UInt16 Revision);

        /// <summary><c>NmGetApiConfiguration</c>Return current API configuration parameters</summary> 
        /// <remarks>
        /// </remarks>
        /// <example>
        /// <code>
        ///    NM_API_CONFIGURATION apiConfig = new NM_API_CONFIGURATION();
        ///    apiConfig.Size = (ushort)System.Runtime.InteropServices.Marshal.SizeOf(typeof(NM_API_CONFIGURATION));
        ///    uint status = NetmonAPI.NmGetApiConfiguration(ref apiConfig);
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="ApiConfig">[out] Struct object for API to fill</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     NM_STATUS_API_VERSION_MISMATCHED: The version of NM_API_CONFIGURATION struct is different
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmGetApiConfiguration(ref NM_API_CONFIGURATION ApiConfig);

        /// <summary><c>NmApiInitialize</c>Overwrite default configuration.</summary> 
        /// <remarks>
        /// Caller needs to provide storage for NmApiConfiguration struct.
        /// </remarks>
        /// <example>
        /// <code>
        ///    NM_API_CONFIGURATION apiConfig = new NM_API_CONFIGURATION();
        ///    apiConfig.Size = (ushort)System.Runtime.InteropServices.Marshal.SizeOf(typeof(NM_API_CONFIGURATION));
        ///    uint status = NetmonAPI.NmGetApiConfiguration(ref apiConfig);
        ///    apiConfig.ThreadingMode = 0;
        ///    status = NetmonAPI.NmApiInitialize(ref apiConfig);
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="ApiConfig">[in] Caller specified API configuration parameter struct</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     NM_STATUS_API_VERSION_MISMATCHED: The version of NM_API_CONFIGURATION struct is different
        ///     ERROR_INVALID_STATE: Cannot change API configuration
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmApiInitialize(ref NM_API_CONFIGURATION ApiConfig);

        /// <summary><c>NmApiClose</c>Release API resources</summary> 
        /// <remarks>
        /// Should be called when done with the API
        /// </remarks>
        /// <example> 
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmApiClose();

        /// <summary><c>NmCloseHandle</c> Release the reference to the object by handle</summary> 
        /// <remarks>
        /// Callers need to close all the object handles returned from API after finish using them.
        /// </remarks>
        /// <exception>None</exception>
        /// <param name="hObjectHandle"> [in] Handle to the object to release </param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>Void</returns>
        [DllImport("NmApi.Dll")]
        public static extern void NmCloseHandle(IntPtr hObjectHandle);
        
        #endregion

        #region Capture Engine Operations

        /// <summary><c>NmOpenCaptureEngine</c>Open a capture engine</summary> 
        /// <remarks>
        /// 
        /// </remarks>
        /// <example>
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="phCaptureEngine">[out] The returned handle to the capture engine object on success</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_INVALID_STATE:     The operation is not available.
        ///     ERROR_ENOUGH_MEMORY: Fail to allocate memory for the object.
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmOpenCaptureEngine(out IntPtr phCaptureEngine);

        /// <summary><c>NmGetAdapterCount</c>Return number of the adapters that the capture engine can access</summary> 
        /// <remarks>
        /// 
        /// </remarks>
        /// <example>
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="ExitMode">[in] The callback function exit mode</param>
        /// <param name="hCaptureEngine">[in] The capture engine under query</param>
        /// <param name="ulCount">[out] The returned count of adapters</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmGetAdapterCount(IntPtr hCaptureEngine, out UInt32 ulCount);

        /// <summary><c>NmGetAdapter</c>Get adapter information from the capture engine</summary> 
        /// <remarks>
        /// Caller can use name, GUID etc. to select adapter to use.  The adapter index should be within the 
        /// Range returned by NmGetAdapterCount method.  Caller needs to provide the storage of the
        /// NmNicAdapterInfo struct.
        /// The fix sized name buffers in the NM_NIC_ADAPTER_INFO structure are marshal to System.char[]. 
        /// Use String nameStr = new String("e.g., whateverDefinedStructName.ConnectionName") to translate the char array to a string object.
        /// </remarks>
        /// <example>
        /// <code>
        ///    IntPtr hCaptureEngine = HandleReturnedByNmOpenCaptureEngine;
        ///    UInt32 ulIndex = 0;
        ///    NM_NIC_ADAPTER_INFO adapterInfo = new NM_NIC_ADAPTER_INFO();
        ///    adapterInfo.Size = (ushort)System.Runtime.InteropServices.Marshal.SizeOf(adapterInfo);
        ///    uint status = NetmonAPI.NmGetAdapter(hCaptureEngine, ulIndex, ref adapterInfo);
        ///    String nameStr = new String(adapterInfo.ConnectionName);
        /// </code>
        /// </example>
        /// <param name="ExitMode">[in] The callback function exit mode</param>
        ///
        /// <exception>None</exception>
        /// <param name="hCaptureEngine">[in] The handle of the capture engine object</param>
        /// <param name="ulIndex">[in] The index number of the adapter to retrieve</param>
        /// <param name="pNMAdapterInfo">[out] The returned adapter information struct</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle
        ///     NM_STATUS_API_VERSION_MISMATCHED: The version of NM_NIC_ADAPTER_INFO struct is different
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmGetAdapter(IntPtr hCaptureEngine, UInt32 ulIndex, ref NM_NIC_ADAPTER_INFO pNMAdapterInfo);

        /// <summary><c>NmConfigAdapter</c>Configure the adapter with the frame indication callback and the caller context.</summary> 
        /// <remarks>
        /// The current callback function and context will overwrite the previous ones.  The adapter index number
        /// Must be in the range returned from NmGetAdapterCount method.
        /// </remarks>
        /// <example> Description
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hCaptureEngine">[in] The handle to the target capture engine</param>
        /// <param name="ulIndex">[in] The index number of the target adapter</param>
        /// <param name="CallbackFunction">[in] The frame indication callback function pointer to set</param>
        /// <param name="pCallerContext">[in] The caller context pointer</param>
        /// <param name="ExitMode">[in] The callback function exit mode</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmConfigAdapter(IntPtr hCaptureEngine, UInt32 ulIndex, CaptureCallbackDelegate CallbackFunction, IntPtr pCallerContext, NmCaptureCallbackExitMode ExitMode);
        
        /// <summary><c>NmStartCapture</c>Start capture on the specified capture engine and adapter</summary> 
        /// <remarks>
        /// Capture mode can be PMODE and LocalOnly.
        /// </remarks>
        /// <example> 
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hCaptureEngine">[in] The handle to the target capture engine</param>
        /// <param name="ulAdapterIndex">[in] The index number of the target adapter</param>
        /// <param name="CaptureMode">[in] The capture mode, PMODE or LOCAL_ONLY</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle
        ///     ERROR_NOT_FOUND: not found capture engine or adapter specified
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmStartCapture(IntPtr hCaptureEngine, UInt32 ulAdapterIndex, NmCaptureMode CaptureMode);

        /// <summary><c>NmPauseCapture</c>Pause the capture</summary> 
        /// <remarks>
        /// 
        /// </remarks>
        /// <example> 
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hCaptureEngine">[in] The handle to the target capture engine</param>
        /// <param name="ulAdapterIndex">[in] The index number of the target adapter</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle
        ///     ERROR_INVALID_STATE: Cannot pause at current state
        ///     ERROR_NOT_FOUND: not found capture engine or adapter specified
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmPauseCapture(IntPtr hCaptureEngine, UInt32 ulAdapterIndex);


        /// <summary><c>NmResumeCapture</c>Resume the capture that is previously paused</summary> 
        /// <remarks>
        /// Cannot resume after NmStopCapture is called.  The frame indication callback is no longer invoked 
        /// Until NmResumeCapture method is called
        /// </remarks>
        /// <example>
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hCaptureEngine">[in] The handle to the target capture engine</param>
        /// <param name="ulAdapterIndex">[in] The index number of the target adapter</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle
        ///     ERROR_INVALID_STATE: Cannot resume at current state
        ///     ERROR_NOT_FOUND: not found capture engine or adapter specified
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmResumeCapture(IntPtr hCaptureEngine, UInt32 ulAdapterIndex);

        /// <summary><c>NmStopCapture</c>Stop capture on given capture engine and adapter</summary> 
        /// <remarks>
        /// The frame indication callback is no longer invoked after this function returns.
        /// </remarks>
        /// <example>
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hCaptureEngine">[in] The handle to the target capture engine</param>
        /// <param name="ulAdapterIndex">[in] The index number of the target adapter</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle
        ///     ERROR_INVALID_STATE: Cannot stop at current state
        ///     ERROR_NOT_FOUND: not found capture engine or adapter specified
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmStopCapture(IntPtr hCaptureEngine, UInt32 ulAdapterIndex);
        #endregion
      
        #region Parsing Functions
        
        /// <summary><c>NmLoadNplParser</c>Load NPL scripts and create NPL parser</summary> 
        /// <remarks>
        /// Callback function is invoked for compile error/warning/info.
        /// </remarks>
        /// <example>
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="pFileName">[in] The start parser script file name</param>
        /// <param name="ulFlags">[in] Option flags</param>
        /// <param name="CallbackFunction">[in] The parser compiler error callback function pointer</param>
        /// <param name="pCallerContext">[in] The caller context pointer that will be passed back to the callback function</param>
        /// <param name="phNplParser">[Out] The returned handle to the NPL parser object</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle
        ///     ERROR_ENOUGH_MEMORY: Fail to create NPL parser object
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmLoadNplParser([MarshalAs(UnmanagedType.LPWStr)] String pFileName, NmNplParserLoadingOption ulFlags, ParserCallbackDelegate CallbackFunction, IntPtr pCallerContext, out IntPtr phNplParser);
        
        /// <summary><c>NmCreateFrameParserConfiguration</c>Create frame parser configuration that contains the filter and field configuration</summary> 
        /// <remarks>
        /// All the frame parser features including conversation and reassembly must be added in the configuration before creating the frame parser.
        /// </remarks>
        /// <example> 
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hNplParser">[in] The handle of the NPL parser used for the frame parser</param>
        /// <param name="CallbackFunction">[in] The compiler error callback function pointer</param>
        /// <param name="pCallerContext">[in] The caller context pointer that will be passed back to the callback function</param>
        /// <param name="phFrameParserConfiguration">[out] The returned handle of the frame parser configuration object</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle
        ///     ERROR_ENOUGH_MEMORY: Fail to create frame parser configuration object.
        ///     ERROR_NOT_FOUND: not found specified NPL parser
        ///     
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmCreateFrameParserConfiguration(IntPtr hNplParser, ParserCallbackDelegate CallbackFunction, IntPtr pCallerContext, out IntPtr phFrameParserConfiguration);
 
        /// <summary><c>NmAddFilter</c>Add filter for optimizing frame parser</summary> 
        /// <remarks>
        /// The filter id is used to evaluate the state of the filter on a parsed frame.
        /// </remarks>
        /// <example>
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hFrameParserConfiguration">[in] The handle of the frame parser configuration object</param>
        /// <param name="pFilterString">[in] The text of the filter</param>
        /// <param name="ulFilterId">[out] The returned filter index in the frame parser</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle
        ///     ERROR_NOT_FOUND: not found specified frame parser configuration
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmAddFilter(IntPtr hFrameParserConfiguration, [MarshalAs(UnmanagedType.LPWStr)] String pFilterString, out UInt32 ulFilterId);

        /// <summary><c>NmAddField</c>Add field for optimizing frame parser</summary> 
        /// <remarks>
        /// All the fields are enumerated in the parsed frame if no field is added.  The field id is used to retrieve the field 
        /// In the parsed frame.  Caller needs to provide unique fully qualified field name, e.g., TCP.Option.Ack.
        /// </remarks>
        /// <example>
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hFrameParserConfiguration">[in] The handle of the frame parser configuration object</param>
        /// <param name="pFieldString">[in] The fully qualified name string of the field</param>
        /// <param name="ulFieldId">[out] The returned field index in the frame parser</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle
        ///     ERROR_NOT_FOUND: not found specified frame parser configuration
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmAddField(IntPtr hFrameParserConfiguration, [MarshalAs(UnmanagedType.LPWStr)] String pFieldString, out UInt32 ulFieldId);

        /// <summary><c>NmAddProperty</c>Add a property to the configuration.</summary> 
        /// <remarks>
        /// The property name should have scope prefix such as Conversation, Global, etc.  If not specified, 
        /// The frame property is the default scope.
        /// </remarks>
        /// <example> This sample shows how to call the NmAddProperty method.
        /// <code>
        ///     IntPtr hFrameParserConfiguration;
        ///     UInt32 myPropID;
        ///     NetmonAPI.NmAddProperty(hFrameParserConfiguration, "Property.TCPPayloadLength", out myPropID);
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hFrameParserConfiguration">[in] Frame Parser Configuration Handle</param>
        /// <param name="pName">[in] Fully qualified name of the property.</param>
        /// <param name="pulPropertyId">[out] Returned ID used to reference the property.</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle or NULL pointer
        ///     ERROR_NOT_FOUND: not found specified frame parser configuration
        ///     ERROR_INVALID_PARAMETER: The specified property name is invalid
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmAddProperty(IntPtr hFrameParserConfiguration, [MarshalAs(UnmanagedType.LPWStr)] String pPropertyString, out UInt32 ulPropertyId);

        /// <summary><c>NmAddSequenceOrderConfig</c>Add protocol sequence order configurations</summary> 
        /// <remarks>
        /// </remarks>
        /// <example> Description
        /// <code>
        ///     
        ///     
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hFrameParserConfiguration">[in] The handle of the frame parser configuration object</param>
        /// <param name="SeqConfig">[in] Caller provided sequence configuration data</param>
        /// <param name="ulConfigId">[out] The retrieval ID of the configuration added to the frame parser configuration</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: NULL pointer
        ///     ERROR_NOT_ENOUGH_MEMORY: Fail to allocate memory to store the configuration.
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmAddSequenceOrderConfig(IntPtr hFrameParserConfiguration, ref NM_PROTOCOL_SEQUENCE_CONFIG SeqConfig, out UInt32 ulConfigId);

        /// <summary><c>NmConfigReassembly</c>Enable or disable reassembly</summary> 
        /// <remarks>
        /// </remarks>
        /// <example> 
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hFrameParserConfiguration">[in] The handle of the frame parser configuration object</param>
        /// <param name="Option">[in] Reassembly options</param>
        /// <param name="bEnable">[in] Action to take, enable or disable</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle or option
        ///     ERROR_NOT_FOUND: not found specified frame parser configuration
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmConfigReassembly(IntPtr hFrameParserConfiguration, NmReassemblyConfigOption Option, Boolean bEnable);

        /// <summary><c>NmConfigConversation</c>Configure conversation options</summary> 
        /// <remarks>
        /// </remarks>
        /// <example> Description
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hFrameParserConfiguration">[in] The handle of the frame parser configuration object</param>
        /// <param name="Option">[in] conversation configuration options</param>
        /// <param name="bEnable">[in] Action to take, enable or disable</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle or option
        ///     ERROR_NOT_FOUND: not found specified frame parser configuration
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmConfigConversation(IntPtr hFrameParserConfiguration, NmConversationConfigOption Option, Boolean bEnable);


        /// <summary><c>NmCreateFrameParser</c>Create frame parser with the given configuration</summary> 
        /// <remarks>
        /// The optimization option is set to NmParserOptimizeNone by default that no optimization is applied.
        /// The existing native applications do not need to recompile.  The new application can take advantage of this flag to 
        /// Force optimization in the scenario where no field is added.  Without this option, the caller can only get a non-optimized 
        /// Parser so that all the fields are included in the parsed frame.  With this option, an optimized frame parser can be
        /// generated to serve the dedicated filtering scenarios.
        /// </remarks>
        /// <example> 
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hFrameParserConfiguration">[in] The handle of the source frame parser configuration object</param>
        /// <param name="phParser">[out] The frame parser</param>
        /// <param name="OptimizeOption">[in] The optimization flag</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle
        ///     ERROR_NOT_FOUND: not found specified frame parser configuration
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmCreateFrameParser(IntPtr hFrameParserConfiguration, out IntPtr phParser, NmFrameParserOptimizeOption OptimizeOption);


        /// <summary><c>NmConfigStartDataType</c>Configure start data type</summary> 
        /// <remarks>
        /// By default, the frame parser starts parsing a frame from the Network Monitor built-in protocol "Frame".
        /// This function lets the caller set the data type to start at.  This is useful for parsing an arbitrary
        /// Data buffer with a frame parser starting from the data type that is configured with this function.
        /// </remarks>
        /// <example> Description
        /// <code>
        ///     
        ///     
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hFrameParserConfiguration">[in] The handle of the target frame parser configuration object</param>
        /// <param name="StartDataTypeName">[in] The name of the data type that the created frame parser starts with</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle
        ///     ERROR_NOT_FOUND: not found specified frame parser configuration
        ///     ERROR_INSUFFICIENT_BUFFER: The given start type name is longer than 260 characters.
        /// </returns>
        /// 
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmConfigStartDataType(IntPtr hFrameParserConfiguration, [MarshalAs(UnmanagedType.LPWStr)] String StartDataTypeName);


        /// <summary><c>NmGetStartDataType</c>Return the start data type of the given frame parser configuration</summary> 
        /// <remarks>
        /// </remarks>
        /// <example>
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hFrameParserConfiguration">[in] The handle of the target frame parser configuration object</param>
        /// <param name="ulBufferLength">[in] The element length of the caller provided buffer</param>
        /// <param name="pBuffer">[out] The name of the data type that the created frame parser starts with</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle
        ///     ERROR_NOT_FOUND: not found specified frame parser configuration
        ///     ERROR_INSUFFICIENT_BUFFER: The given buffer is not big enough to hold the start data type name string.
        /// </returns>
        /// 
        [DllImport("NmApi.Dll")]
        unsafe public static extern UInt32 NmGetStartDataType(IntPtr hFrameParserConfiguration, UInt32 ulBufferLength, char* pBuffer);


        #endregion

        #region Parsed Frame Operations
        /// <summary><c>NmParseFrame</c>Parse the raw Network Monitor frame and return it in parsed format</summary> 
        /// <remarks>
        /// The parsed frame contains the frame information, filter state and enumeration of field.  When reassembly is
        /// Enabled, the last fragment of the payload completing the reassembly process and insert the reasembled raw frame.
        /// The ulFrameNumber parameter is for conversation or global properties if using frame number as the key.  If the same
        /// Frame number is used for different frames, the properties' values may be overwritten by the last instance.
        /// </remarks>
        /// <example>
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hFrameParser">[in] The handle of the frame parser used to parse the Given frame object</param>
        /// <param name="hRawFrame">[in] The handle of the target frame to parser</param>
        /// <param name="ulFrameNumber">[in] The frame number should be used in parsing process if enabled by option flag</param>
        /// <param name="Options">[in] See flag definition NmFrameParsingOption</param>
        /// <param name="phParsedFrame">[out] The handle to the result parsed frame object</param>
        /// <param name="phInsertedRawFrame">[out] the handle of inserted raw frame as the result of parsing, e.g., reassembly</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle
        ///     ERROR_NOT_FOUND: not found specified frame parser or raw frame
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmParseFrame(IntPtr hFrameParser, IntPtr hRawFrame, UInt32 ulFrameNumber, NmFrameParsingOption Options, out IntPtr phParsedFrame, out IntPtr phInsertedRawFrame);

        
        /// <summary><c>NmParseBuffer</c>Parse the given data buffer and return it in parsed format</summary> 
        /// <remarks>
        /// The data buffer contains the byte array that can be a raw frame, part of raw frame or any arbitrary data.
        /// The parsed frame contains the fabricated frame information. The filter state and enumeration of field are supported.
        /// The inter frame reassembly is not supported since it requires multiple frames and conversation that are 
        /// Not available in buffer mode.
        /// </remarks>
        /// <example>
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hParser">[in] The handle of the frame parser used to parse the Given frame object</param>
        /// <param name="DataBuffer">[in] The pointer to the target data buffer</param>
        /// <param name="ulBufferLength">[in] length of the data buffer in previous parameter</param>
        /// <param name="ulFrameNumber">[in] The frame number should be used in parsing process if enabled by option flag</param>
        /// <param name="Options">[in] See flag definition NmFrameParsingOption</param>
        /// <param name="phParsedFrame">[out] The handle to the result parsed frame object</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle
        ///     ERROR_NOT_FOUND: not found specified frame parser or raw frame
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmParseBuffer(IntPtr hParser, Byte[] DataBuffer, UInt32 ulBufferLength, UInt32 ulFrameNumber, NmFrameParsingOption Options, out IntPtr phParsedFrame);

        /// <summary><c>NmBuildRawFrameFromBuffer</c>Build a raw frame using a given data buffer</summary> 
        /// <remarks>
        /// The data buffer is transformed to a raw frame object.  The media type, time stamp are optional.  Their default
        /// Values are zero.
        /// </remarks>
        /// <example>
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="DataBuffer">[in] The pointer to the target data buffer</param>
        /// <param name="ulBufferLength">[in] length of the data buffer in previous parameter</param>
        /// <param name="ulMedia">[in] Media type of the target raw frame</param>
        /// <param name="ullTimeStamp">[in] Capture time stamp of the target raw frame</param>
        /// <param name="phRawFrame">[out] The handle to the result parsed frame object</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle
        ///     ERROR_ENOUGH_MEMORY: No space to build the new frame
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmBuildRawFrameFromBuffer(IntPtr DataBuffer, UInt32 ulBufferLength, UInt32 ulMedia, UInt64 ullTimeStamp, out IntPtr phRawFrame);

        /// <summary><c>NmBuildRawFrameFromBufferEx</c>Build a raw frame using a given data buffer</summary> 
        /// <remarks>
        /// Same as NmBuildRawFrameFromBufferEx but stores time zone information
        /// </remarks>
        /// <example>
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="pDataBuffer">[in] The pointer to the target data buffer</param>
        /// <param name="ulBufferLength">[in] length of the data buffer in previous parameter</param>
        /// <param name="ulMedia">[in] Media type of the target raw frame</param>
        /// <param name="pTime">[in] Capture time information of the target raw frame</param>
        /// <param name="phRawFrame">[out] The handle to the result parsed frame object</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle or NULL pointer
        ///     ERROR_NOT_ENOUGH_MEMORY: No space to build the new frame
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmBuildRawFrameFromBufferEx(IntPtr DataBuffer, UInt32 ulBufferLength, UInt32 ulMedia, ref NM_TIME pTime, out IntPtr phRawFrame);

        /// <summary><c>NmGetFrameFragmentInfo</c>Return fragment information of the given parsed frame</summary> 
        /// <remarks>
        /// Raw frame does not aware of its fragment type. Only parsing the frame can tell when reassembly is enabled.
        /// </remarks>
        /// <example> 
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hParsedFrame">[in] The handle of the target parsed frame</param>
        /// <param name="FragmentationInfo">[out] Caller provided struct pointer</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle
        ///     ERROR_NOT_FOUND: not found specified parsed frame
        ///     ERROR_INSUFFICIENT_BUFFER: If the protocol name length is longer than the buffer in PNmReassemblyInfo struct
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmGetFrameFragmentInfo(IntPtr hParsedFrame, ref NmFragmentationInfo FragmentationInfo);

        /// <summary><c>NmGetFilterCount</c>Return configured filter count in the given frame parser</summary> 
        /// <remarks>
        /// 
        /// </remarks>
        /// <example>
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hFrameParser">[in] frame parser under inspection</param>
        /// <param name="ulFilterCount">[out] number of filters of the given frame parser.  It is zero if return code is not success</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle
        ///     ERROR_NOT_FOUND: not found specified frame parser
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmGetFilterCount(IntPtr hFrameParser, out UInt32 ulFilterCount);

        /// <summary><c>NmEvaluateFilter</c>Return the state of specified filter in given parsed frame</summary> 
        /// <remarks>
        /// 
        /// </remarks>
        /// <example> 
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hParsedFrame">[in] The handle of the target parsed frame to evaluate</param>
        /// <param name="ulFilterId">[in] The identify number of the filter</param>
        /// <param name="bPassFilter">[out] The filter evaluation result.  TRUE means pass; FALSE means not pass</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle
        ///     ERROR_NOT_FOUND: not found specified parsed frame
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmEvaluateFilter(IntPtr hParsedFrame, UInt32 ulFilterId, out Boolean bPassFilter);

        /// <summary><c>NmGetFieldCount</c>Return number of fields enumerated in the given parsed frame</summary> 
        /// <remarks>
        /// 
        /// </remarks>
        /// <example>
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hParsedFrame">[in] The handle of the target frame</param>
        /// <param name="ulFieldCount">[out] The number of fields returned in parsed frame</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle
        ///     ERROR_NOT_FOUND: not found specified parsed frame
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmGetFieldCount(IntPtr hParsedFrame, out UInt32 ulFieldCount);

        /// <summary><c>NmGetParsedFieldInfo</c>Return the field information of a parsed frame specified by field index number</summary> 
        /// <remarks>
        /// The pointer to field is valid until the parsed frame containing the field is closed.
        /// </remarks>
        /// <example>
        /// <code>
        ///     NmParsedFieldInfo parsedDataField = new NmParsedFieldInfo();
        ///     parsedDataField.Size = (ushort)System.Runtime.InteropServices.Marshal.SizeOf(typeof(NmParsedFieldInfo));
        ///     NetmonAPI.NmGetParsedFieldInfo(hParsedFrame, 0, 0, ref parsedDataField);
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hParsedFrame">[in] The handle of the parsed frame</param>
        /// <param name="ulFieldId">[in] The identify number of the field to get</param>
        /// <param name="ulOption">[in] The retrieve flag</param>
        /// <param name="pParsedFieldInfo">[out] The pointer to the parsed field</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle
        ///     ERROR_NOT_FOUND: not found specified parsed frame or field
        ///     ERROR_ARITHMETIC_OVERFLOW: The field length is greater than 65535
        ///     NM_STATUS_API_VERSION_MISMATCHED: The pParsedFieldInfo.Size is not initialized or it is different from the version of the API version.
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmGetParsedFieldInfo(IntPtr hParsedFrame, UInt32 ulFieldId, UInt32 ulOption, ref NmParsedFieldInfo pParsedFieldInfo);

        /// <summary><c>NmGetParsedFieldInfoEx</c>Return the field information of a parsed frame specified by field index number</summary> 
        /// <remarks>
        /// The pointer to field is valid until the parsed frame containing the field is closed.
        /// </remarks>
        /// <example>
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hParsedFrame">[in] The handle of the parsed frame</param>
        /// <param name="ulFieldId">[in] The identify number of the field to get</param>
        /// <param name="ulOption">[in] The retrieve flag</param>
        /// <param name="pParsedFieldInfo">[out] The pointer to the parsed field buffer</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle or NULL pointer
        ///     ERROR_NOT_FOUND: not found specified parsed frame or field
        ///     NM_STATUS_API_VERSION_MISMATCHED: The pParsedFieldInfo.Size is not initialized or it is different from the version of the API version.
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmGetParsedFieldInfoEx(IntPtr hParsedFrame, UInt32 ulFieldId, UInt32 ulOption, ref NmParsedFieldInfoEx pParsedFieldInfoEx);

        /// <summary><c>NmGetFieldName</c>Return the name property of the parsed field specified by field id</summary> 
        /// <remarks>
        /// ulBufferLength is element count.
        /// </remarks>
        /// <example> Description
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hParsedFrame">[in] The handle of the target parsed frame</param>
        /// <param name="ulFieldId">[in] The identify number of the field to get</param>
        /// <param name="RequestedName">[in] The enum to select intended name property</param>
        /// <param name="ulBufferLength">[in] The element length of caller provided buffer length</param>
        /// <param name="pBuffer">[out] The caller provided buffer</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle or NULL pointer
        ///     ERROR_NOT_FOUND: not found specified parsed frame or field
        ///     ERROR_INSUFFICIENT_BUFFER: If ulBufferLength is shorted than the name length
        /// </returns>
        [DllImport("NmApi.Dll", CharSet=CharSet.Unicode ) ]
        unsafe public static extern UInt32 NmGetFieldName(IntPtr hParsedFrame, UInt32 ulFieldId, NmParsedFieldNames RequestedName, UInt32 ulBufferLength, char* pBuffer);

        /// <summary><c>NmGetFieldOffsetAndSize</c>Return the offset and size of the field specified by field id</summary> 
        /// <remarks>
        /// The returned field size is in unit of byte
        /// </remarks>
        /// <example> Description
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hParsedFrame">[in] The handle of the target parsed frame</param>
        /// <param name="ulFieldId">[in] The identify number of the field</param>
        /// <param name="ulFieldOffset">[out] The pointer to the returned field offset</param>
        /// <param name="ulFieldSize">[out] The pointer to the returned field size</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle
        ///     ERROR_NOT_FOUND: not found specified parsed frame or field
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmGetFieldOffsetAndSize(IntPtr hParsedFrame, UInt32 ulFieldId, out UInt32 ulFieldOffset, out UInt32 ulFieldSize);

        /// <summary><c>NmGetFieldValueNumber8Bit</c>Return 8-bit field value</summary> 
        /// <remarks>
        /// </remarks>
        /// <example>
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hParsedFrame">[in] The handle of the target parsed frame</param>
        /// <param name="ulFieldId">[in] The identify number of the field</param>
        /// <param name="ubNumber">[out] The value of the requested field</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle
        ///     ERROR_NOT_FOUND: not found specified parsed frame or field
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmGetFieldValueNumber8Bit(IntPtr hParsedFrame, UInt32 ulFieldId, out Byte ubNumber);

        /// <summary><c>NmGetFieldValueNumber16Bit</c>Return 16-bit field value</summary> 
        /// <remarks>
        /// </remarks>
        /// <example>
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hParsedFrame">[in] The handle of the target parsed frame</param>
        /// <param name="ulFieldId">[in] The identify number of the field</param>
        /// <param name="uiNumber">[out] The value of the requested field</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle or NULL pointer
        ///     ERROR_NOT_FOUND: not found specified parsed frame or field
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmGetFieldValueNumber16Bit(IntPtr hParsedFrame, UInt32 ulFieldId, out UInt16 uiNumber);

        /// <summary><c>NmGetFieldValueNumber32Bit</c>Return 32-bit field value</summary> 
        /// <remarks>
        /// </remarks>
        /// <example>
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hParsedFrame">[in] The handle of the target parsed frame</param>
        /// <param name="ulFieldId">[in] The identify number of the field</param>
        /// <param name="ulNumber">[out] The value of the requested field</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle
        ///     ERROR_NOT_FOUND: not found specified parsed frame or field
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmGetFieldValueNumber32Bit(IntPtr hParsedFrame, UInt32 ulFieldId, out UInt32 ulNumber);

        /// <summary><c>NmGetFieldValueNumber64Bit</c>Return 64-bit field value</summary> 
        /// <remarks>
        /// </remarks>
        /// <example>
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hParsedFrame">[in] The handle of the target parsed frame</param>
        /// <param name="ulFieldId">[in] The identify number of the field</param>
        /// <param name="ullNumber">[out] The value of the requested field</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle
        ///     ERROR_NOT_FOUND: not found specified parsed frame or field
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmGetFieldValueNumber64Bit(IntPtr hParsedFrame, UInt32 ulFieldId, out UInt64 ullNumber);

        /// <summary><c>NmGetFieldValueByteArray</c>Return byte array field value in buffer</summary> 
        /// <remarks>
        /// </remarks>
        /// <example>
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hParsedFrame">[in] The handle of the target parsed frame</param>
        /// <param name="ulFieldId">[in] The identify number of the field</param>
        /// <param name="ulByteLength">[in] The length of the provided buffer in byte</param>
        /// <param name="pBuffer">[out] The value of the requested field</param>
        /// <param name="ulReturnLength">[out] The number of bytes actaully copied</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle
        ///     ERROR_NOT_FOUND: not found specified parsed frame or field
        /// </returns>
        [DllImport("NmApi.Dll")]
        unsafe public static extern UInt32 NmGetFieldValueByteArray(IntPtr hParsedFrame, UInt32 ulFieldId, UInt32 ulByteLength, byte* pBuffer, out UInt32 ulReturnLength);

        /// <summary><c>NmGetFieldValueString</c>Return string field value in buffer</summary> 
        /// <remarks>
        /// </remarks>
        /// <example>
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hParsedFrame">[in] The handle of the target parsed frame</param>
        /// <param name="ulFieldId">[in] The identify number of the field</param>
        /// <param name="ulBufferLength">[in] The element length of the provided buffer</param>
        /// <param name="pValueBuffer">[out] The value of the requested field</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle
        ///     ERROR_NOT_FOUND: not found specified parsed frame or field
        /// </returns>
        [DllImport("NmApi.Dll")]
        unsafe public static extern UInt32 NmGetFieldValueString(IntPtr hParsedFrame, UInt32 ulFieldId, UInt32 ulBufferLength, char* pValueBuffer);

        /// <summary><c>NmGetFieldInBuffer</c>Get the field in user provided buffer</summary> 
        /// <remarks>
        /// Only the content up to the buffer length is copied.  Caller may call NmGetFieldOffsetAndSize to get the size 
        /// Before calling this function with proper buffer length.
        /// </remarks>
        /// <example>
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hParsedFrame">[in] The handle of the target parsed frame</param>
        /// <param name="ulFieldId">[in] The identify number of the field</param>
        /// <param name="ulBufferLength">[in] The element length of caller provided buffer</param>
        /// <param name="pFieldBuffer">[out] caller provided buffer</param>
        /// <param name="ulReturnLength">[out] actual number of byte copied</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle or NULL pointer
        ///     ERROR_NOT_FOUND: not found specified parsed frame or field
        ///     ERROR_INSUFFICIENT_BUFFER: Not enough space in buffer, data is not copied.
        /// </returns>
        [DllImport("NmApi.Dll")]
        unsafe public static extern UInt32 NmGetFieldInBuffer(IntPtr hParsedFrame, UInt32 ulFieldId, UInt32 ulBufferLength, byte* pFieldBuffer, out UInt32 ulReturnLength);

        /// <summary><c>NmGetRequestedPropertyCount</c>Get the number of properties added to the parser.</summary> 
        /// <remarks>
        /// None.
        /// </remarks>
        /// <example> This sample shows how to call the NmGetRequestedPropertyCount method.
        /// <code>
        ///     IntPtr FrameParser;    // returned by NmCreateFrameParser
        ///     UInt32 PropertyCount = UInt32.Zero;
        ///     NetmonAPI.NmGetRequestedPropertyCount(FrameParser, out PropertyCount);
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hFrameParser">[in] Frame Parser Configuration Handle</param>
        /// <param name="ulCount">[out] Count of properties added to this frame configuration.</param>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle
        ///     ERROR_NOT_FOUND: not found specified frame parser
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmGetRequestedPropertyCount(IntPtr hFrameParser, out UInt32 ulCount);

        /// <summary><c>NmGetPropertyInfo</c>Return info structure for a specific property by ID.</summary> 
        /// <remarks>
        /// When the property container type is multi-value storage, the value type and size may be unknown if the property name added does not contain the key.
        /// Since the size is unknown, the caller may need to call the retrieval function twice with the proper buffer of required size returned by the 
        /// Retrieval function which first returned ERROR_INSUFFICIENT_BUFFER.  The same is true for array properties when the index in not included in the property string.
        ///
        /// If the property container type is unknown, the property is not available for retrieval.
        /// </remarks>
        /// <example> This sample shows how to call the NmGetParsedPropertyInfo method.  The method returns
        ///           The property name string size in NM_NPL_PROPERTY_INFO.NameSize.
        /// <code>
        ///     IntPtr FrameParser;    // returned by NmCreateFrameParser
        ///     UInt32 PropertyId = 2; // returned by add property method
        ///     NM_NPL_PROPERTY_INFO PropertyInfo = new NM_NPL_PROPERTY_INFO();
        ///     PropertyInfo.Size = (ushort)Marshal.SizeOf(typeof(NM_NPL_PROPERTY_INFO));
        ///     PropertyInfo.Name = String.Empty;
        ///     NetmonAPI.NmGetPropertyInfo(FrameParser, PropertyId, ref PropertyInfo);
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hFrameParser">[in] Frame Parser Configuration Handle</param>
        /// <param name="ulPropertyId">[in] ID of the property returned from NmAddProperty</param>
        /// <param name="pInfo">[out] Information of the property specified by ID.</param>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle or NULL pointer
        ///     ERROR_NOT_FOUND: Not found specified property
        ///     ERROR_INSUFFICIENT_BUFFER: Buffer size is not sufficient.  The required size is returned in NameLength
        ///     NM_STATUS_API_VERSION_MISMATCHED: NM_PARSED_PROPERTY_INFO version mismatch
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmGetPropertyInfo(IntPtr hFrameParser, UInt32 ulPropertyId, ref NM_NPL_PROPERTY_INFO pInfo);

        /// <summary><c>NmGetPropertyById</c>Return property value by ID.</summary> 
        /// <remarks>
        /// The Key for multi-value storage properties or Index for array properties must not provide both the property name and key index array.
        /// The key type must match the type used in NPL parser.
        /// If no key is added, set ulKeyCount to zero.
        /// </remarks>
        /// <example>.
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hFrameParser">[in] Frame Parser Handle</param>
        /// <param name="ulPropertyId">[in] ID of the property returned from NmAddProperty</param>
        /// <param name="ulBufferSize">[in] Size of the buffer supplied in byte count.</param>
        /// <param name="pBuffer">[out] Buffer for returned data.</param>
        /// <param name="ulReturnLength">[out] Size of the data returned.</param>
        /// <param name="ulType">[out] Value type of the returned MVS property.</param>
        /// <param name="ulKeyCount">[in] Number of keys provided</param>
        /// <param name="pKeyArray">[in] key Array to look up in MVS and property group </param>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle or NULL pointer
        ///     ERROR_NOT_FOUND: not found specified property
        ///     ERROR_INSUFFICIENT_BUFFER: Not enough space in buffer, data is not copied.
        /// </returns>
        [DllImport("NmApi.Dll")]
        unsafe public static extern UInt32 NmGetPropertyById(IntPtr hFrameParser, UInt32 ulPropertyId, UInt32 ulBufferSize, byte* pBuffer, out UInt32 ulReturnLength, out NmPropertyValueType ulType, UInt32 ulKeyCount, CNmPropertyStorageKey [] pKeyArray);

        /// <summary><c>NmGetPropertyByName</c>Return property value by Name.</summary> 
        /// <remarks>
        /// The property is not necessarily added to the frame parser configuration if a non-optimized frame parser is used.  In this case, the property id is not available and the
        /// The property name can be used.  The full qualified name must be used as to add the property to the frame parser configuration.
        /// The index or key must not provided by both name and key array. If no key to add, set ulKeyCount to zero.
        /// </remarks>
        /// <example>
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hFrameParser">[in] Frame Parser Handle</param>
        /// <param name="pPropertyName">[in] full qualified name of the property </param>
        /// <param name="ulBufferSize">[in] Size of the buffer supplied in byte count.</param>
        /// <param name="pBuffer">[out] Buffer for returned data.</param>
        /// <param name="pulReturnLength">[out] Size of the data returned.</param>
        /// <param name="ulType">[out] Value type of the returned MVS property.</param>
        /// <param name="ulKeyCount">[in] Number of keys provided</param>
        /// <param name="pKeyArray">[in] key Array to look up in MVS and property group </param>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle or NULL pointer
        ///     ERROR_NOT_FOUND: not found specified property
        ///     ERROR_INSUFFICIENT_BUFFER: Not enough space in buffer, data is not copied.
        /// </returns>
        [DllImport("NmApi.Dll")]
        unsafe public static extern UInt32 NmGetPropertyByName(IntPtr hFrameParser, [MarshalAs(UnmanagedType.LPWStr)] String pPropertyName, UInt32 ulBufferSize, byte* pBuffer, out UInt32 pulReturnLength, out NmPropertyValueType ulType, UInt32 ulKeyCount, CNmPropertyStorageKey [] pKeyArray);

        #endregion

        #region Raw Frame Operations
        /// <summary><c>NmGetRawFrameLength</c>Return length of the raw frame</summary> 
        /// <remarks>
        /// 
        /// </remarks>
        /// <example> 
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hFrame">[in] The handle of the target raw frame</param>
        /// <param name="pulLength">[out] Frame length</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle
        ///     ERROR_NOT_FOUND: not found specified raw frame
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmGetRawFrameLength(IntPtr hFrame, out UInt32 pulLength);

        /// <summary><c>NmGetRawFrame</c>Return raw frame</summary> 
        /// <remarks>
        /// The frame buffer is valid until the raw frame is closed.
        /// </remarks>
        /// <example>
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hFrame">[in] The handle of the target raw frame</param>
        /// <param name="ulLength">[in] Caller buffer length in byte element</param>
        /// <param name="pFrameBuffer"> [out] Buffer for raw frame data by caller</param>
        /// <param name="pulReturnLength">[out] Actual returned data length</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle
        ///     ERROR_NOT_FOUND: not found specified raw frame
        /// </returns>
        [DllImport("NmApi.Dll")]
        unsafe public static extern UInt32 NmGetRawFrame(IntPtr hFrame, UInt32 ulLength, byte* pFrameBuffer, out UInt32 pulReturnLength);

        /// <summary><c>NmGetPartialRawFrame</c>Return partial frame data in caller provided buffer</summary> 
        /// <remarks>
        /// Use caller provided offset and buffer length to copy the frame data.
        /// </remarks>
        /// <example>
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hRawFrame">[in] The handle of the target raw frame</param>
        /// <param name="ulFrameOffset">[in] Start offset to copy</param>
        /// <param name="ulBufferLength">[in] Caller buffer length, the Number of bytes to copy</param>
        /// <param name="pFrameBuffer">[out] Caller provided buffer</param>
        /// <param name="pulReturnLength">[out] Actual returned data length</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle
        ///     ERROR_NOT_FOUND: not found specified raw frame
        /// </returns>
        [DllImport("NmApi.Dll")]
        unsafe public static extern UInt32 NmGetPartialRawFrame(IntPtr hRawFrame, UInt32 ulFrameOffset, UInt32 ulBufferLength, byte* pFrameBuffer, out UInt32 pulReturnLength);

        /// <summary><c>NmGetFrameMacType</c>Return MAC type of the frame</summary> 
        /// <remarks>
        /// </remarks>
        /// <example>
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hFrame">[in] The handle of a parsed or a raw frame object</param>
        /// <param name="pulMacType">[out] The pointer to the MAC type of the frame</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle
        ///     ERROR_INVALID_PARAMETER: hFrame is not a parsed or a raw frame handle.
        ///     ERROR_NOT_FOUND: not found specified frame
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmGetFrameMacType(IntPtr hFrame, out UInt32 pulMacType);


        /// <summary><c>NmGetFrameTimeStamp</c>Return the local time stamp of the frame</summary> 
        /// <remarks>
        /// </remarks>
        /// <example> 
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hFrame"> [in] The handle of a parsed or a raw frame object</param>
        /// <param name="pTimeStamp"> [out] The pointer to the local time stamp of the frame.</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle
        ///     ERROR_INVALID_PARAMETER: hFrame is not a parsed or a raw frame handle.
        ///     ERROR_NOT_FOUND: not found specified frame
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmGetFrameTimeStamp(IntPtr hFrame, out UInt64 pTimeStamp);

        /// <summary><c>NmGetFrameTimeStampEx</c>Return the extended time information of the capture</summary> 
        /// <remarks>
        /// </remarks>
        /// <example> 
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hFrame"> [in] The handle of a parsed or a raw frame object</param>
        /// <param name="pTime"> [out] The pointer to the NM_TIME of the frame</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle
        ///     ERROR_INVALID_PARAMETER: hFrame is not a parsed or a raw frame handle.
        ///     ERROR_NOT_FOUND: not found specified frame
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmGetFrameTimeStampEx(IntPtr hFrame, ref NM_TIME pTime);

        /// <summary><c>NmGetFrameCommentInfo</c>Return the frame comment title and description</summary>
        /// <remarks>
        /// If the buffer passed in is NULL, the buffer length arguments will indicate the required length.
        /// </remarks>
        /// <example>
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hFrame"> [in] The handle of the raw frame object></param>
        /// <param name="ulCommentTitleBufferLength"> [inout] The pointer to the actual byte length that corresponds to the title buffer</param>
        /// <param name="pCommentTitleBuffer"> [in] Caller supplied buffer to hold the comment title</param>
        /// <param name="ulCommentDescriptionBufferLength"> [inout] The pointer to the actual byte length that corresponds to the description buffer</param>
        /// <param name="pCommentDescriptionBuffer"> [in] Caller supplied buffer to hold the comment description</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle or NULL pointer
        ///     ERROR_NOT_FOUND: Specified parsed frame not found
        ///     ERROR_INSUFFICIENT_BUFFER: If either of the supplied buffers is NULL
        ///     ERROR_EMPTY: Frame comment information was not found
        /// </returns>
        [DllImport("NmApi.Dll")]
        unsafe public static extern UInt32 NmGetFrameCommentInfo(IntPtr hFrame, ref UInt32 ulCommentTitleBufferLength, byte* pCommentTitleBuffer, ref UInt32 ulCommentDescriptionBufferLength, byte* pCommentDescriptionBuffer);

        #endregion

        #region Capture File Operations

        /// <summary><c>NmCreateCaptureFile</c> Create a new Network Monitor capture file for adding frames.</summary> 
        /// <remarks>
        /// This is the capture file to write to. Close it by calling NmCloseObjHandle method.
        /// The file can be opened in 2 modes specified by Flags, wrap around (default) and chain capture.
        /// </remarks>
        /// <example> Description
        /// <code>
        ///     
        ///     
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="pFileName"> [in] The name of the file to create</param>
        /// <param name="ulSize"> [in] The caller specified maximum size of the file in byte.  The hard limit is 500 MByte</param>
        /// <param name="ulFlags"> [in] Specify the file modes, wrap-round or chain capture</param>
        /// <param name="phCaptureFile"> [out] The returned handle to the capture file object if successful</param>
        /// <param name="ulReturnSize"> [out] The actual size of the file in byte.</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: NULL pointer
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmCreateCaptureFile([MarshalAs(UnmanagedType.LPWStr)] String pFileName, UInt32 ulSize, NmCaptureFileFlag ulFlags, out IntPtr phCaptureFile, out UInt32 ulReturnSize);

        /// <summary><c>NmOpenCaptureFile</c> Open a Network Monitor capture file to read</summary> 
        /// <remarks>
        /// The file is read only. Close capture file by calling NmCloseObjHandle method.
        /// </remarks>
        /// <example> 
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="pFileName"> [in] The name of the file to open</param>
        /// <param name="phCaptureFile"> [out]The returned handle of the capture file object if successful</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: NULL pointer
        ///     ERROR_NOT_FOUND: not found specified file
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmOpenCaptureFile([MarshalAs(UnmanagedType.LPWStr)] String pFileName, out IntPtr phCaptureFile);

        /// <summary><c>NmOpenCaptureFileInOrder</c> Open a Network Monitor capture file to read</summary> 
        /// <remarks>
        /// The frame in file are in the order of the sequence specified in the frame parser parameter
        /// The file is read only. Close capture file by calling NmCloseObjHandle method.
        /// </remarks>
        /// <example> 
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="pFileName"> [in] The name of the file to open</param>
        /// <param name="pOrderParser"> [in]The struct containing the frame parser configured with sequnece order instructions</param>
        /// <param name="phCaptureFile"> [out]The returned handle of the capture file object if successful</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: NULL pointer
        ///     ERROR_NOT_FOUND: not found specified file
        ///     ERROR_INVALID_PARAMETER: frame parser does not have sequence configuration.
        ///     ERROR_NOT_ENOUGH_MEMORY: not enough memory to build required objects.
        ///     NM_STATUS_API_VERSION_MISMATCHED: PNM_ORDER_PARSER_PARAMETER version does not match.
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmOpenCaptureFileInOrder([MarshalAs(UnmanagedType.LPWStr)] String pFileName, ref NM_ORDER_PARSER_PARAMETER pOrderParser, out IntPtr phCaptureFile);

        /// <summary><c>NmAddFrame</c> Add a frame to the specified capture file.</summary> 
        /// <remarks>
        /// The target capture file must be opened with NmCreateCaptureFile method
        /// </remarks>
        /// <example> 
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hCaptureFile"> [in] The destination capture file for the frame</param>
        /// <param name="hFrame"> [in] The handle of the frame to add</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle
        ///     ERROR_NOT_FOUND: not found specified file or frame
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmAddFrame(IntPtr hCaptureFile, IntPtr hFrame);


        /// <summary><c>NmGetFrameCount</c> Get frame count in the specified capture file</summary> 
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception>None</exception>
        /// <param name="hCaptureFile"> [in] The target capture file under query</param>
        /// <param name="hFrameCount"> [out] Return frame count</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle
        ///     ERROR_NOT_FOUND: not found specified capture file
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmGetFrameCount(IntPtr hCaptureFile, out UInt32 hFrameCount);

        /// <summary><c>NmGetFrame</c> Get frame by number from the specified capture file.</summary> 
        /// <remarks>
        /// The frame number is the index number in the capture file.
        /// </remarks>
        /// <exception>None</exception>
        /// <param name="hCaptureFile"> [in] Handle to the capture file</param>
        /// <param name="ulFrameNumber"> [in] Frame number in the capture file to retrieve</param>
        /// <param name="phFrame"> [out] The returned handle to the raw frame object if successful</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS: Frame handle is valid
        ///     ERROR_BAD_ARGUMENTS: Invalid handle
        ///     ERROR_NOT_FOUND: not found specified capture file or frame
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmGetFrame(IntPtr hCaptureFile, UInt32 ulFrameNumber, out IntPtr phFrame);
        #endregion

        #region NPL Property Extension

        /// <summary><c>NmGetTopConversation</c>Return the top level conversation and protocol name.</summary> 
        /// <remarks>
        /// The protocol name length is returned to caller.  So if the provided buffer is not enough, caller
        /// Can call again with the proper sized buffer.
        /// </remarks>
        /// <example> This sample shows how to call the NmGetTopConversation method.
        /// <code>
        ///     IntPtr hParsedFrame;
        ///     UInt32 returnLength;
        ///     UInt32 conversationId;
        ///     Char[] retName = null;
        ///     unsafe
        ///     {
        ///        retName = new Char[256];
        ///        fixed (char* buffer = retName)
        ///        {
        ///           NetmonAPI.NmGetTopConversation(
        ///                          hParsedFrame, 
        ///                          256, 
        ///                          buffer, 
        ///                          out returnLength,
        ///                          out conversationId);
        ///        }
        ///     }
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hParsedFrame">[in] Parsed Frame</param>
        /// <param name="ulBufferESize">[in] Size of the for protocol name in WCHAR.</param>
        /// <param name="pProtocolName">[out] Buffer for protocol name.</param>
        /// <param name="pulProtocolNameLength">[out] Not include terminator in WCHAR.</param>
        /// <param name="pulConversationID">[out] ID of the TOP Level Conversation</param>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle or NULL pointer
        ///     ERROR_INSUFFICIENT_BUFFER: Insufficient buffer space
        ///     ERROR_NOT_FOUND: not found specified parsed frame
        /// </returns>
        [DllImport("NmApi.Dll")]
        unsafe public static extern UInt32 NmGetTopConversation(IntPtr hParsedFrame, UInt32 ulBufferESize, char* pProtocolName, out UInt32 pulProtocolNameLength, out UInt32 pulConversationID);

        /// <summary><c>NmGetParentConversation</c>Return parent conversation information of the given conversation.</summary> 
        /// <remarks>
        /// The parent protocol name length is returned to caller.  So if the provided buffer is not enough, caller
        /// Can call again with the proper sized buffer.
        /// </remarks>
        /// <example> This sample shows how to call the NmGetParentConversation method.
        /// <code>
        ///     IntPtr myParsedFrame;
        ///     UInt32 protocolId = 24;
        ///     UInt32 returnLength;
        ///     UInt32 parentConvID;
        ///     Char[] retName = null;
        ///     unsafe
        ///     {
        ///        retName = new Char[256];
        ///        fixed (char* buffer = retName)
        ///        {
        ///           NetmonAPI.NmGetParentConversation(
        ///                             myParsedFrame,
        ///                             protocolId,
        ///                             256, 
        ///                             buffer, 
        ///                             out returnLength,
        ///                             out parentConvID);
        ///        }
        ///     }
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="myParsedFrame">[in] Parsed Frame</param>
        /// <param name="ulConversationID">[in] ID of the Conversation you want the parent of.</param>
        /// <param name="ulBufferESize">[in] Buffer size for the Parent protocol name in WCHAR count.</param>
        /// <param name="pParentProtocolName">[out] Buffer for the Parent Protocol Name. </param>
        /// <param name="ulParentProtocolNameLength">[out] Returned Length of Parent Protocol Name in WCHAR.</param>
        /// <param name="ulParentConversationID">[out] Size of the for protocol name.</param>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: Invalid handle or NULL pointer
        ///     ERROR_INSUFFICIENT_BUFFER: Insufficient buffer space
        ///     ERROR_NOT_FOUND: not found specified frame parser
        /// </returns>
        [DllImport("NmApi.Dll")]
        unsafe public static extern UInt32 NmGetParentConversation(IntPtr hParsedFrame, UInt32 ulConversationId, UInt32 ulBufferESize, char* pParentProtocolNameBuffer, out UInt32 ulParentProtocolNameLength, out UInt32 ulParentConversationID);

        #endregion

        #region Parser Profile Operations

        /// <summary><c>NmLoadWithNplProfile</c>Create frame parser using the provided Guid for an NPL profile</summary> 
        /// <remarks>
        /// The ulFlags is reserved for future options.
        /// </remarks>
        /// <example> 
        /// <code>
        /// </code>
        /// </example>
        /// <exception>None</exception>
        /// <param name="pProfileGuid">[in] The GUID of the profile to use.</param>
        /// <param name="ulFlags">[in] Option flags</param>
        /// <param name="CallbackFunction">[in] The compiler error callback function pointer</param>
        /// <param name="pCallerContext">[in] The caller context pointer that will be passed back to the callback function</param>
        /// <param name="phNplParser">[Out] The returned handle to the NPL parser object</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS: Successfully compiled NPL
        ///     ERROR_BAD_ARGUMENTS: Invalid handle or NULL pointer
        ///     ERROR_NOT_ENOUGH_MEMORY: Fail to create frame parser configuration object.
        ///     ERROR_NOT_FOUND: The given profile GUID does not exist.
        ///     ERROR_NO_MATCH: the provided GUID does not exist.
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmLoadWithNplProfile([MarshalAs(UnmanagedType.LPWStr)] String pProfileGuid, NmNplParserLoadingOption ulFlags, ParserCallbackDelegate CallbackFunction, IntPtr pCallerContext, out IntPtr phNplParser);

        /// <summary><c>NmCreateNplProfile</c>adds a profile given using given parameters.</summary> 
        /// <remarks>
        /// This method allows developers to create profiles much like when the UI builds a profile.
        /// The simplest case does not provide the optional argument TemplateID.  In this case, a new profile is 
        /// created using the paths that have been provided.  No sparser is generated, and no directories are formed.
        ///
        /// The more complex case provides a TemplateID that corresponds to an existing profile.  The include path for
        /// the existing profile will be duplicated for the new profile (which is why the provided IncludePath is
        /// ignored in this case).  Profiles created in this way will have a directory created using the GUID of the 
        /// profile if successful. When these profiles are deleted, all files present in the created directory will 
        /// also be removed.  Template profiles gain an additional search path to the user's local APPDATA as well
        /// as an explicit include my_sparser.npl in the generated sparser.npl.
        /// </remarks>
        /// <example>
        /// <code>
        ///
        /// // Building a blank profile.
        /// NetmonAPI.NmCreateNplProfile( "ProfileTest00-1", "This profile is created, verified, and deleted.", 
        ///                                    "c:\\somePath", "MyGuid" );
        ///
        /// // Building a profile from a template using the GUID of a previously existing profile.
        /// NetmonAPI.NmCreateNplProfile( "ProfileTest00-2", "This profile is created from another profile.", 
        ///                                    NULL, "MyGuid2", "AD161723-4281-4d33-804E-5E43EE61D163" );
        ///
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="Name">[in] The name of the profile. </param>
        /// <param name="Description">[in] The description of the profile. </param>
        /// <param name="IncludePath">[in] A semicolon delimited list of the paths to search in when building parsers.
        ///  This argument is not used when a templateID is provided.</param>
        /// <param name="Guid">[in] The GUID for the profile.</param>
        /// <param name="TemplateID">[in] Optional. Defaults to NULL. If provided, the newly created profile will be 
        ///  built as a template of the given profile. </param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS: Successfully added the profile.
        ///     ERROR_ALREADY_ASSIGNED: The provided GUID is already present.
        ///     ERROR_NO_MATCH: The provided TemplateID does not exist.
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmCreateNplProfile([MarshalAs(UnmanagedType.LPWStr)] String Name, [MarshalAs(UnmanagedType.LPWStr)] String Description,
                                                             [MarshalAs(UnmanagedType.LPWStr)] String IncludePath, [MarshalAs(UnmanagedType.LPWStr)] String Guid,
                                                             [MarshalAs(UnmanagedType.LPWStr)] String templateID);

        /// <summary><c>NmGetNplProfileAttribute</c>Retrieves the profile attribute using the index of the profile.</summary> 
        /// <remarks>
        /// This method can be used to retrieve an attribute value as a string for a given profile.  If the given buffer is too small,
        /// the method will populate as much as it can, and then change the value of ulBufferLenth to the required number of bytes to 
        /// retrieve the entire string.
        /// </remarks>
        /// <example>
        /// <code>
        ///     String myBuffer = new String('0', 1024);
        ///     UInt32 myIndex = 1;
        ///     UInt32 myBufferSize = 1024;
        ///     NetmonAPI.NmGetNplProfileAttribute( myIndex, NmNplProfileAttributePackageGuid, ref myBufferSize, out myBuffer );
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="pProfileGuid">[in] the index of the profile</param>
        /// <param name="attribute">[in] the attribute enum that corresponds to the string we are looking for</param>
        /// <param name="ulBufferELength">[inout] The length of the buffer we would like to fill</param>
        /// <param name="pAttributeBuffer">[out] The buffer to place the string in.</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS: Successfully retrieved the attribute.
        ///     ERROR_NO_MATCH: The provided GUID does not match an existing profile's GUID.
        ///     ERROR_INSUFFICIENT_BUFFER: The provided buffer is too short.  The ulBufferELenth will be updated.
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmGetNplProfileAttribute(UInt32 ulIndex, NmNplProfileAttribute attribute, ref UInt32 ulBufferELength, [MarshalAs(UnmanagedType.LPWStr)] String pAttributeBuffer);

        /// <summary><c>NmGetNplProfileAttributeByGuid</c>Retrieves the profile attribute using the GUID of the profile.</summary> 
        /// <remarks>
        /// This method can be used to retrieve an attribute value as a string for a given profile.  If the given buffer is too small,
        /// the method will populate as much as it can, and then change the value of ulBufferLenth to the required number of bytes to 
        /// retrieve the entire string.
        /// </remarks>
        /// <example>
        /// <code>
        ///     String guid = "AD161723-4281-4d33-804E-5E43EE61D163";
        ///     String myBuffer = new String('0', 1024);
        ///     UInt32 myBufferSize = 1024;
        ///     NetmonAPI.NmGetNplProfileAttributeByGuid( guid, NmNplProfileAttributePackageGuid, ref myBufferSize, out myBuffer );
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="pProfileGuid">[in] the GUID of the profile</param>
        /// <param name="attribute">[in] the attribute enum that corresponds to the string we are looking for</param>
        /// <param name="ulBufferELength">[inout] The length of the buffer we would like to fill</param>
        /// <param name="pAttributeBuffer">[out] The buffer to place the string in.</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS: Successfully retrieved the attribute.
        ///     ERROR_NO_MATCH: The provided GUID does not match an existing profile's GUID.
        ///     ERROR_INSUFFICIENT_BUFFER: The provided buffer is too short.  The ulBufferELenth will be updated.
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmGetNplProfileAttributeByGuid([MarshalAs(UnmanagedType.LPWStr)] String pProfileGuid, NmNplProfileAttribute attribute, ref UInt32 ulBufferELength, [MarshalAs(UnmanagedType.LPWStr)] String pAttributeBuffer);

        /// <summary><c>NmSetNplProfileAttribute</c>Sets the profile's attribute using the index of the profile.</summary> 
        /// <remarks>
        /// This method can be used to set an attribute on a profile.  There are only three attributes that can be 
        /// modified on an existing profile; the Name, the Description, and the Include Path.  This method accepts the index of
        /// profile to modify.  
        /// </remarks>
        /// <example>
        /// <code>
        /// // Load the first profile.
        /// NetmonAPI.NmSetNplProfileAttribute( 0, NmNplProfileAttributeDescription, "TestValue" );
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="ulIndex">[in] the index of the profile to use..</param>
        /// <param name="attribute">[in] the enumeration item that corresponds to the desired attribute.</param>
        /// <param name="pAttributeBuffer">[in] The buffer to populate the attribute from.</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS: The attribute has been successfully updated.
        ///     ERROR_NO_MATCH: The provided index is not found in the Parser Profile Manager.
        ///     ERROR_INVALID_ACCESS: The profile is read only and cannot be modified.
        ///     ERROR_BAD_ARGUMENTS: The enumeration item was not Description, Name, or IncludePath.
        /// </returns>
        /// 
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmSetNplProfileAttribute( UInt32 ulIndex, NmNplProfileAttribute attribute, [MarshalAs(UnmanagedType.LPWStr)] String pAttributeBuffer );

        /// <summary><c>NmSetNplProfileAttributeByGuid</c>Sets the profile's attribute using the GUID of the profile.</summary> 
        /// <remarks>
        /// This method can be used to set an attribute on a profile.  There are only three attributes that can be 
        /// modified on an existing profile; the Name, the Description, and the Include Path.
        /// </remarks>
        /// <example>
        /// <code>
        /// NetmonAPI.NmSetNplProfileAttributeByGuid( "AD161723-4281-4d33-804E-5E43EE61D163", NmNplProfileAttributeDescription, "TestValue" );
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="ulIndex">[in] the GUID of the profile to update.</param>
        /// <param name="attribute">[in] the enumeration item that corresponds to the desired attribute.</param>
        /// <param name="pAttributeBuffer">[in] The buffer to populate the attribute from.</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS: The attribute has been successfully updated.
        ///     ERROR_NO_MATCH: The provided GUID is not found in the Parser Profile Manager..
        ///     ERROR_INVALID_ACCESS: The profile is read only and cannot be modified.
        ///     ERROR_BAD_ARGUMENTS: The enumeration item was not Description, Name, or IncludePath.
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmSetNplProfileAttributeByGuid( [MarshalAs(UnmanagedType.LPWStr)] String pProfileGuid, NmNplProfileAttribute attribute, [MarshalAs(UnmanagedType.LPWStr)] String pAttributeBuffer );
        
        /// <summary><c>NmGetNplProfileCount</c>retrieves the number of profiles available</summary> 
        /// <remarks>
        /// The number of profiles includes the User Defined Profiles and the Installed Profiles.
        /// This number will always be greater than zero.  If no profiles are present, the parser engine
        /// will construct the pure capture profile when the ParserProfileManager is initialized which 
        /// will be done when this method is called.
        /// </remarks>
        /// <example>
        /// <code>
        /// UInt32 myProfileCount = 0;
        /// NetmonAPI.NmGetNplProfileCount( out myProfileCount );
        /// </code>
        /// </example>
        /// <exception>None</exception>
        /// <param name="pulCount">[out] Number of profiles in the ParserProfileManager.</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS: Method will always return success.
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmGetNplProfileCount( out UInt32 pulCount );

        /// <summary><c>NmGetActiveNplProfileGuid</c>retrieves the GUID for the currently active profile</summary> 
        /// <remarks>
        /// If the provided buffer is not long enough to contain the active NPL Profile's Id, the method will return
        /// ERROR_INSUFFICIENT_BUFFER and the ulBufferELength will be assigned to the necessary size to read the buffer.
        /// </remarks>
        /// <example>
        /// <code>
        /// String myGuid = new String('0', 50);
        /// UInt32 myGuidLength = 50;
        /// NetmonAPI.NmGetActiveNplProfileGuid( ref myGuidLength, out myGuid );
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="ulBufferELength">[in] The length of the provided buffer.</param>
        /// <param name="pProfileGuid">[out] The buffer to place the GUID of the active profile into. </param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS: The GUID has been retrieved.
        ///     ERROR_NO_MATCH: There is no active Npl Profile assigned.
        ///     ERROR_INSUFFICIENT_BUFFER: the provided buffer is not long enough to contain the GUID.
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmGetActiveNplProfileGuid( ref UInt32 ulBufferELength, [MarshalAs(UnmanagedType.LPWStr)] String pProfileGuid );

        /// <summary><c>NmSetActiveNplProfile</c>Sets the active profile to the given ID</summary> 
        /// <remarks>
        /// Method will attempt to compile the provided profile. If compilation is not successful, then ERROR_CAN_NOT_COMPLETE will be
        /// returned. In this case, users should use NmLoadWithNplProfile to determine what errors were encountered.  When successful,
        /// the profile is set as active for all Microsoft Network Monitor clients that share the HKCU key. (netmon.exe/nmcap.exe/etc)
        /// </remarks>
        /// <example>
        /// <code>
        ///   String MicrosoftFasterNPLProfileID = "AD161723-4281-4d33-804E-5E43EE61D163";
        ///   NetmonAPI.NmSetActiveNplProfile( MicrosoftFasterNPLProfileID );
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="pProfileGuid">[in] </param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS: The profile has been set as active.
        ///     ERROR_NO_MATCH: The GUID is not present.
        ///     ERROR_CAN_NOT_COMPLETE: The provided profile cannot successfully compile.
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmSetActiveNplProfile( [MarshalAs(UnmanagedType.LPWStr)] String pProfileGuid );

        /// <summary><c>NmDeleteNplProfile</c></summary> 
        /// <remarks>
        /// Method will delete a profile if the profile can be deleted. Some profiles cannot be deleted.
        /// Profiles that cannot be deleted include: 
        ///            Installed Profiles from Microsoft/3rd party packages.
        ///            The currently active profile.
        /// </remarks>
        /// <example>
        /// <code>
        ///   String MicrosoftFasterNPLProfileID = "AD161723-4281-4d33-804E-5E43EE61D163";
        ///   NetmonAPI.NmDeleteNplProfile( MicrosoftFasterNPLProfileID );
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="pProfileGuid">[in] </param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS: The profile has been successfully deleted.
        ///     ERROR_NO_MATCH: The provided GUID is not available for deletion.
        ///     ERROR_ALREADY_ASSIGNED: The provided GUID could not be deleted because it is currently active.
        ///     ERROR_INVALID_ACCESS: The profile is read only.
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmDeleteNplProfile([MarshalAs(UnmanagedType.LPWStr)] String pProfileGuid);

        #endregion

        #region Driver Capture Filter Operations

        //////////////////////////////////////////////
        ///
        /// Driver capture filter
        ///
        /// OVERVIEW: You can specify a driver filter per adapter, per process.  The driver filtering consists of
        /// a set of filters where each filter contain a set blocks.  Each block contains a set of OLP,
        /// (Offset, Length, Pattern), expressions which also contain an operand.  The operation, AND or OR, is fixed 
        /// for each block of OLP expressions.  And the operation between blocks is the opposite of the operation 
        /// between each OLP.  So if you set the block operation to AND, the OLP operation is OR.  The operation 
        /// between filters can be AND or OR.  The order of the operations for filters occurs in the order you add 
        /// them.  The order of operations for blocks and OLP expressions depends on the index you specify when you 
        /// add them.
        ///
        /// OLP expressions, blocks and filters can also be "short circuited" using the NmOlpActionFlags.  This allows
        /// you to optimize an expression when you know the evaluation in the driver will not be sufficient to
        /// completely evaluate the intended filter operation accurately.  For instance, consider a filter where you are 
        /// looking for a TCP port. In some cases the port offset can change when IPv4 options exist.  This short circuit
        /// provides a way to return the frame early rather than evaluating the rest of the expression.
        /// 
        /// You must add a driver filter before the capture engine is started.  Driver filtering can affect your
        /// capturing performance as a larger delay in the driver doing these evaluations may cause you to drop
        /// frames.  You can use NmGetLiveCaptureFrameCounts to monitor dropped frames and other statistics.

        /// <summary><c>NmCreateOlp</c>Create OLP for the capture filter</summary> 
        /// <remarks>
        /// Build an OLP expression and return the OLP ID unique in process scope.
        ///
        /// The OLP pattern must be contained in the byte array as it should be in frames in terms of byte alignment.
        /// For example, if the bit offset is 34 and bit length is 17, the pattern is 0b10101010101010101, the
        /// Representation in the given buffer should be 0b00101010,10101010,10100000 or 0x2AAAA0 (3 bytes).
        ///                                                  -------------------
        /// Note the two leading zeros and five zeros on the back.  But the bits in front or rear of the pattern is not 
        /// Required to be set to zero.
        /// 
        /// Given bit offset BO an bit length BL, the byte array length should be:
        /// (((BO % 8) + BL)/8 + (((BO + BL) % 8) == 0)? 0:1)
        /// The byte array length in above example: 
        /// (((34 % 8) + 17)/8 + (((34 + 17) % 8) == 0)? 0:1) is 3
        /// 
        /// If the byte array passed in is shorter, the behavior is unpredictable or most likely an access violation.
        /// </remarks>
        /// <example>
        /// <code>
        /// Here is an example of TCP syn flag OLP that allows only TCP syn packets.  This assumes that the underlying 
        /// protocols match Frame.Ethernet.IPv4.TCP for the frame you evaluate.
        ///
        /// UInt32 BitOffset = 446; 
        /// UInt32 BitLength = 1; 
        /// System.Byte[] TcpFlagSyn = 0x02; // Syn flag bit is the 7th from the MSB (0b00000010).
        /// UInt32 OlpId;
        /// UInt32 status = NmCreateOlp(BitOffset, BitLength, TcpFlagSyn, 
        ///                             NmFilterMatchMode.NmFilterMatchModeEqual, 
        ///                             NmOlpActionFlags.NmOlpActionFlagsNone, out OlpId);
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="BitOffset">[in] The bit offset of the OLP</param>
        /// <param name="BitLength">[in] The bit length of the OLP</param>
        /// <param name="pPattern">[in] The pattern of the OLP.</param>
        /// <param name="OpMode">[in] The comparison operator type, e.g., EQ, NOT EQ, GREATER, LESS, etc.</param>
        /// <param name="Options">[in] The option configuration flags defined by NmOlpActionFlags</param>
        /// <param name="pulOlpId">[out] The process unique ID for the OLP.</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: NULL pointers or zero BitLength.
        ///     ERROR_ARITHMETIC_OVERFLOW: If more than 4G OLP entities are created, the OLP id overflows.
        ///     ERROR_NOT_ENOUGH_MEMORY: Fail to allocate memory for the OLP entity object.
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmCreateOlp(UInt32 BitOffset, 
                                                UInt32 BitLength, 
                                                System.Byte[] pPattern, 
                                                NmFilterMatchMode OpMode, 
                                                NmOlpActionFlags Options, 
                                                out UInt32 pulOlpId);

        /// <summary><c>NmCreateOlpBlock</c>Create OLP block for the capture filter </summary> 
        /// <remarks>
        /// Create an OLP block and return it's ID which is unique in processes scope.  The ulConditionCount specifies 
        /// the maximum number of the OLP conditions the block can hold.  The ulConditionCount must be non-zero.
        /// </remarks>
        /// <example>
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="ulConditionCount">[in] The number of conditions the block can hold</param>
        /// <param name="ulOptions">[in] Opyional NmOlpActionFlags flags </param>
        /// <param name="pulOlpBlockId">[out] The unique ID of the OLP block</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: NULL pointer
        ///     ERROR_ARITHMETIC_OVERFLOW: If more than 4G OLP entities are created, the OLP id overflows.
        ///     ERROR_NOT_ENOUGH_MEMORY: Fail to allocate memory for the OLP entity object.
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmCreateOlpBlock(UInt32 ulConditionCount, NmOlpActionFlags ulOptions, out UInt32 pulOlpBlockId);

        /// <summary><c>NmAddOlpToBlock</c>Add the OLP to the OLP block </summary> 
        /// <remarks>
        /// Add the OLP condition to the OLP block.  The ulOlpBlockId must reference an OLP block entity that was
        /// created by NmCreateOlpBlock.  The ulOlpId must reference to an OLP condition entity created by NmCreateOlp.
        /// </remarks>
        /// <example>
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="ulOlpBlockId">[in] The target OLP block ID created by NmCreateOlpBlock</param>
        /// <param name="ulIndex">[in] The zero based index position of the OLP in the target block</param>
        /// <param name="ulOlpId">[in] The ID of the OLP to add returned from NmCreateOlpl</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: The specified index is invalid; or the OLP types referenced by the IDs are wrong.
        ///     ERROR_NOT_FOUND: The specified block or OLP are not found.
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmAddOlpToBlock(UInt32 ulOlpBlockId, UInt32 ulIndex, UInt32 ulOlpId);

        /// <summary><c>NmCreateOlpFilter</c>Create the OLP driver capture filter</summary> 
        /// <remarks>
        /// Create an OLP filter entity and return its ID that is unique in processes scope.  
        /// The ulBlockCount specifies the maximum number of the OLP blocks the 
        /// filter created can hold.  The ulBlockCount must be non-zero.
        /// </remarks>
        /// <example>
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="ulBlockCount">[in] The number of OLP blocks the filter can hold</param>
        /// <param name="ulOptions">[in] The filter configuration flags.  NmFilterOptionFlags and NmOlpActionFlags can be combined</param>
        /// <param name="pulOlpFilterId">[out] The unique ID of the new OLP filter</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: NULL pointer.
        ///     ERROR_ARITHMETIC_OVERFLOW: If more than 4G OLP entities are created, the OLP id overflows.
        ///     ERROR_NOT_ENOUGH_MEMORY: Fail to allocate memory for the OLP entity object.
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmCreateOlpFilter(UInt32 ulBlockCount, UInt32 ulOptions, out UInt32 pulOlpFilterId);

        /// <summary><c>NmAddOlpBlockToFilter</c>Add the OLP block to the OLP filter</summary> 
        /// <remarks>
        /// Add the OLP block to the OLP filter.  The ulOlpFilterId must reference to an OLP filter entity that is created by
        /// NmCreateOlpFilter.  The ulOlpBlockId must reference to an OLP block entity created by NmCreateOlpBlock.
        /// </remarks>
        /// <example>
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="ulOlpFilterId">[in] The ID of the target OLP filter created by NmCreateOlpFilter.</param>
        /// <param name="ulIndex">[in] The index position of the OLP block in the target OLP filter</param>
        /// <param name="ulOlpBlockId">[in] The ID of the OLP block to add created by NmCreateOlpBlock.</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BAD_ARGUMENTS: The specified index is invalid; or the OLP types referenced by the ids are wrong.
        ///     ERROR_NOT_FOUND: The specified filter or block are not found.
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmAddOlpBlockToFilter(UInt32 ulOlpFilterId, UInt32 ulIndex, UInt32 ulOlpBlockId);

        /// <summary><c>NmDeleteOlpEntity</c>Delete an OLP entity specified by its ID</summary> 
        /// <remarks>
        /// Delete the specified OLP entity, (filter, or block, or OLP expression), from API OLP entity pool so the deleted entity
        /// can no longer used to construct new driver filters. The OLP entity here is not the actual driver capture Filter but 
        /// the layered logical objects that can be added to the driver using NmAddDriverCaptureFilter, or deleted from the driver 
        /// using NmDeleteDriverCaptureFilter.
        ///
        /// Do not delete an OLP entity before the filter containing it has been added to the driver, otherwise 
        /// the filter will not be built correctly.
        ///
        /// All OLP entities (OLP condition, block and filter) can be deleted after the filter is added to the driver.
        ///
        /// </remarks>
        /// <example>
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="ulEntityId">[in] The unique ID of the OLP entity: OLP expression, block or filter.</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_NOT_FOUND: The specified ulEntityId is invalid.
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmDeleteOlpEntity(UInt32 ulEntityId);

        /// <summary><c>NmAddDriverCaptureFilter</c>Add capture filter to the adapter.</summary> 
        /// <remarks>
        /// Add the capture filter created with NmCreateOlpFilter to the driver. This function must be called when  
        /// the adapter is not actively capturing.
        /// 
        /// There is a limit to the number of OLP conditions that can be added to the driver.  This limit is per 
        /// adapter for each API process. The default limit is 16.  If the limit of OLP conditions are exceeded, 
        /// ERROR_INVALID_PARAMETER is returned.  The limit can be changed by modifying the DWORD in the registry, 
        /// HKLM\SYSTEM\CurrentControlSet\Services\nm3\OlpFilterConditionMaxCount. 
        /// </remarks>
        /// <example>
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hCaptureEngine">[in] The capture engine handle returned from NmOpenCaptureEngine.</param>
        /// <param name="ulAdapterIndex">[in] The adapter the filter was added to.</param>
        /// <param name="ulFilterId">[in] The filter id returned from NmCreateOlpFilter and used to reference the OLP Filter.</param>
        /// <param name="ulOption">[in] The option defined for filter configuration.  Reserved.</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_INVALID_PARAMETER: Invalid handle, or driver parameter validation failed, or filter is invalid (no OLP inside).
        ///     ERROR_NOT_ENOUGH_MEMORY: No memory to construct filter.
        ///     ERROR_NO_SYSTEM_RESOURCES: Too many OLP in driver or not enough resource for the operation.
        ///     ERROR_BUSY: The driver is not in the proper state for this operation.
        ///     ERROR_OBJECT_ALREADY_EXISTS: The specified filter already exists in driver.
        ///     ERROR_NOT_FOUND: not found specified capture engine, adapter or filter
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmAddDriverCaptureFilter(IntPtr hCaptureEngine, UInt32 ulAdapterIndex, UInt32 ulFilterId, UInt32 ulOption);

        /// <summary><c>NmDeleteDriverCaptureFilter</c>Remove capture filter from the driver </summary> 
        /// <remarks>
        /// Removes the driver capture filter specified by filter ID created by NmCreateOlpFilter.
        /// The filter will no longer available afterwards.  All entities are also cleaned up and
        /// no longer available.
        ///
        /// This function must be called when the adapter is not actively capturing.
        ///
        /// All driver capture filters are deleted from driver when the application terminates.
        /// </remarks>
        /// <example>
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hCaptureEngine">[in] The capture engine handle returned by NmOpenCaptureEngine.</param>
        /// <param name="ulAdapterIndex">[in] The adapter the filter is to be removed from</param>
        /// <param name="ulFilterId">[in] The capture filter id to delete, created by NmCreateOlpFilter.</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_BUSY: The driver is not in the proper state for this operation.
        ///     ERROR_NOT_FOUND: not found specified capture engine, adapter or filter.
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmDeleteDriverCaptureFilter(IntPtr hCaptureEngine, UInt32 ulAdapterIndex, UInt32 ulFilterId);

        /// <summary><c>NmGetLiveCaptureFrameCounts</c>Return the frame counters of the adapter</summary> 
        /// <remarks>
        ///     The counters in PNM_CAPTURE_STATISTICS are reported for the specified adapter.
        ///     If there are multiple capture engines capture on the same adapter, they share the same counter set.
        /// </remarks>
        /// <example>
        /// <code>
        /// </code>
        /// </example>
        ///
        /// <exception>None</exception>
        /// <param name="hCaptureEngine">[in] The capture engine handle returned by NmOpenCaptureEngine.</param>
        /// <param name="ulAdapterIndex">[in] The adapter index.</param>
        /// <param name="pCaptureStatistics">[out] The capture statistics of the specified adapter.</param>
        /// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
        /// <returns>
        ///     ERROR_SUCCESS:
        ///     ERROR_NOT_FOUND: not found specified adapter.
        ///     ERROR_BAD_ARGUMENTS: Invalid handle or NULL pointer.
        /// </returns>
        [DllImport("NmApi.Dll")]
        public static extern UInt32 NmGetLiveCaptureFrameCounts(IntPtr hCaptureEngine, UInt32 ulAdapterIndex, ref NM_CAPTURE_STATISTICS pCaptureStatistics);

        #endregion
    }
}

