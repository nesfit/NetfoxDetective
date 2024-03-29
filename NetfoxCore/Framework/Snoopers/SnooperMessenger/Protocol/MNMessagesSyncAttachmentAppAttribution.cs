// Copyright (c) 2017 Jan Pluskal, Viliam Letavay
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

/**
 * Autogenerated by Thrift Compiler (0.9.3)
 *
 * DO NOT EDIT UNLESS YOU ARE SURE THAT YOU KNOW WHAT YOU ARE DOING
 *  @generated
 */

using System;
using System.Collections.Generic;
using System.Text;
using Thrift.Protocol;

namespace Netfox.Snoopers.SnooperMessenger.Protocol
{

  #if !SILVERLIGHT
  [Serializable]
  #endif
  public partial class MNMessagesSyncAttachmentAppAttribution : TBase
  {
    private long _AttributionAppId;
    private string _AttributionMetadata;
    private string _AttributionAppName;
    private string _AttributionAppIconURI;
    private string _AndroidPackageName;
    private long _IOSStoreId;
    private Dictionary<long, long> _OtherUserAppScopedFbIds;
    private MNMessagesSyncAppAttributionVisibility _Visibility;

    public long AttributionAppId
    {
      get
      {
        return _AttributionAppId;
      }
      set
      {
        __isset.AttributionAppId = true;
        this._AttributionAppId = value;
      }
    }

    public string AttributionMetadata
    {
      get
      {
        return _AttributionMetadata;
      }
      set
      {
        __isset.AttributionMetadata = true;
        this._AttributionMetadata = value;
      }
    }

    public string AttributionAppName
    {
      get
      {
        return _AttributionAppName;
      }
      set
      {
        __isset.AttributionAppName = true;
        this._AttributionAppName = value;
      }
    }

    public string AttributionAppIconURI
    {
      get
      {
        return _AttributionAppIconURI;
      }
      set
      {
        __isset.AttributionAppIconURI = true;
        this._AttributionAppIconURI = value;
      }
    }

    public string AndroidPackageName
    {
      get
      {
        return _AndroidPackageName;
      }
      set
      {
        __isset.AndroidPackageName = true;
        this._AndroidPackageName = value;
      }
    }

    public long IOSStoreId
    {
      get
      {
        return _IOSStoreId;
      }
      set
      {
        __isset.IOSStoreId = true;
        this._IOSStoreId = value;
      }
    }

    public Dictionary<long, long> OtherUserAppScopedFbIds
    {
      get
      {
        return _OtherUserAppScopedFbIds;
      }
      set
      {
        __isset.OtherUserAppScopedFbIds = true;
        this._OtherUserAppScopedFbIds = value;
      }
    }

    public MNMessagesSyncAppAttributionVisibility Visibility
    {
      get
      {
        return _Visibility;
      }
      set
      {
        __isset.Visibility = true;
        this._Visibility = value;
      }
    }


    public Isset __isset;
    #if !SILVERLIGHT
    [Serializable]
    #endif
    public struct Isset {
      public bool AttributionAppId;
      public bool AttributionMetadata;
      public bool AttributionAppName;
      public bool AttributionAppIconURI;
      public bool AndroidPackageName;
      public bool IOSStoreId;
      public bool OtherUserAppScopedFbIds;
      public bool Visibility;
    }

    public MNMessagesSyncAttachmentAppAttribution() {
    }

    public void Read (TProtocol iprot)
    {
      iprot.IncrementRecursionDepth();
      try
      {
        TField field;
        iprot.ReadStructBegin();
        while (true)
        {
          field = iprot.ReadFieldBegin();
          if (field.Type == TType.Stop) { 
            break;
          }
          switch (field.ID)
          {
            case 1:
              if (field.Type == TType.I64) {
                AttributionAppId = iprot.ReadI64();
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 2:
              if (field.Type == TType.String) {
                AttributionMetadata = iprot.ReadString();
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 3:
              if (field.Type == TType.String) {
                AttributionAppName = iprot.ReadString();
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 4:
              if (field.Type == TType.String) {
                AttributionAppIconURI = iprot.ReadString();
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 5:
              if (field.Type == TType.String) {
                AndroidPackageName = iprot.ReadString();
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 6:
              if (field.Type == TType.I64) {
                IOSStoreId = iprot.ReadI64();
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 7:
              if (field.Type == TType.Map) {
                {
                  OtherUserAppScopedFbIds = new Dictionary<long, long>();
                  TMap _map67 = iprot.ReadMapBegin();
                  for( int _i68 = 0; _i68 < _map67.Count; ++_i68)
                  {
                    long _key69;
                    long _val70;
                    _key69 = iprot.ReadI64();
                    _val70 = iprot.ReadI64();
                    OtherUserAppScopedFbIds[_key69] = _val70;
                  }
                  iprot.ReadMapEnd();
                }
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 8:
              if (field.Type == TType.Struct) {
                Visibility = new MNMessagesSyncAppAttributionVisibility();
                Visibility.Read(iprot);
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            default: 
              TProtocolUtil.Skip(iprot, field.Type);
              break;
          }
          iprot.ReadFieldEnd();
        }
        iprot.ReadStructEnd();
      }
      finally
      {
        iprot.DecrementRecursionDepth();
      }
    }

    public void Write(TProtocol oprot) {
      oprot.IncrementRecursionDepth();
      try
      {
        TStruct struc = new TStruct("MNMessagesSyncAttachmentAppAttribution");
        oprot.WriteStructBegin(struc);
        TField field = new TField();
        if (__isset.AttributionAppId) {
          field.Name = "AttributionAppId";
          field.Type = TType.I64;
          field.ID = 1;
          oprot.WriteFieldBegin(field);
          oprot.WriteI64(AttributionAppId);
          oprot.WriteFieldEnd();
        }
        if (AttributionMetadata != null && __isset.AttributionMetadata) {
          field.Name = "AttributionMetadata";
          field.Type = TType.String;
          field.ID = 2;
          oprot.WriteFieldBegin(field);
          oprot.WriteString(AttributionMetadata);
          oprot.WriteFieldEnd();
        }
        if (AttributionAppName != null && __isset.AttributionAppName) {
          field.Name = "AttributionAppName";
          field.Type = TType.String;
          field.ID = 3;
          oprot.WriteFieldBegin(field);
          oprot.WriteString(AttributionAppName);
          oprot.WriteFieldEnd();
        }
        if (AttributionAppIconURI != null && __isset.AttributionAppIconURI) {
          field.Name = "AttributionAppIconURI";
          field.Type = TType.String;
          field.ID = 4;
          oprot.WriteFieldBegin(field);
          oprot.WriteString(AttributionAppIconURI);
          oprot.WriteFieldEnd();
        }
        if (AndroidPackageName != null && __isset.AndroidPackageName) {
          field.Name = "AndroidPackageName";
          field.Type = TType.String;
          field.ID = 5;
          oprot.WriteFieldBegin(field);
          oprot.WriteString(AndroidPackageName);
          oprot.WriteFieldEnd();
        }
        if (__isset.IOSStoreId) {
          field.Name = "IOSStoreId";
          field.Type = TType.I64;
          field.ID = 6;
          oprot.WriteFieldBegin(field);
          oprot.WriteI64(IOSStoreId);
          oprot.WriteFieldEnd();
        }
        if (OtherUserAppScopedFbIds != null && __isset.OtherUserAppScopedFbIds) {
          field.Name = "OtherUserAppScopedFbIds";
          field.Type = TType.Map;
          field.ID = 7;
          oprot.WriteFieldBegin(field);
          {
            oprot.WriteMapBegin(new TMap(TType.I64, TType.I64, OtherUserAppScopedFbIds.Count));
            foreach (long _iter71 in OtherUserAppScopedFbIds.Keys)
            {
              oprot.WriteI64(_iter71);
              oprot.WriteI64(OtherUserAppScopedFbIds[_iter71]);
            }
            oprot.WriteMapEnd();
          }
          oprot.WriteFieldEnd();
        }
        if (Visibility != null && __isset.Visibility) {
          field.Name = "Visibility";
          field.Type = TType.Struct;
          field.ID = 8;
          oprot.WriteFieldBegin(field);
          Visibility.Write(oprot);
          oprot.WriteFieldEnd();
        }
        oprot.WriteFieldStop();
        oprot.WriteStructEnd();
      }
      finally
      {
        oprot.DecrementRecursionDepth();
      }
    }

    public override string ToString() {
      StringBuilder __sb = new StringBuilder("MNMessagesSyncAttachmentAppAttribution(");
      bool __first = true;
      if (__isset.AttributionAppId) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("AttributionAppId: ");
        __sb.Append(AttributionAppId);
      }
      if (AttributionMetadata != null && __isset.AttributionMetadata) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("AttributionMetadata: ");
        __sb.Append(AttributionMetadata);
      }
      if (AttributionAppName != null && __isset.AttributionAppName) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("AttributionAppName: ");
        __sb.Append(AttributionAppName);
      }
      if (AttributionAppIconURI != null && __isset.AttributionAppIconURI) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("AttributionAppIconURI: ");
        __sb.Append(AttributionAppIconURI);
      }
      if (AndroidPackageName != null && __isset.AndroidPackageName) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("AndroidPackageName: ");
        __sb.Append(AndroidPackageName);
      }
      if (__isset.IOSStoreId) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("IOSStoreId: ");
        __sb.Append(IOSStoreId);
      }
      if (OtherUserAppScopedFbIds != null && __isset.OtherUserAppScopedFbIds) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("OtherUserAppScopedFbIds: ");
        __sb.Append(OtherUserAppScopedFbIds);
      }
      if (Visibility != null && __isset.Visibility) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("Visibility: ");
        __sb.Append(Visibility== null ? "<null>" : Visibility.ToString());
      }
      __sb.Append(")");
      return __sb.ToString();
    }

  }

}
