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
  public partial class MNMessagesSyncDeltaDeliveryReceipt : TBase
  {
    private MNMessagesSyncThreadKey _ThreadKey;
    private long _ActorFbId;
    private string _DeviceId;
    private long _AppId;
    private long _TimestampMs;
    private List<string> _MessageIds;
    private long _DeliveredWatermarkTimestampMs;

    public MNMessagesSyncThreadKey ThreadKey
    {
      get
      {
        return _ThreadKey;
      }
      set
      {
        __isset.ThreadKey = true;
        this._ThreadKey = value;
      }
    }

    public long ActorFbId
    {
      get
      {
        return _ActorFbId;
      }
      set
      {
        __isset.ActorFbId = true;
        this._ActorFbId = value;
      }
    }

    public string DeviceId
    {
      get
      {
        return _DeviceId;
      }
      set
      {
        __isset.DeviceId = true;
        this._DeviceId = value;
      }
    }

    public long AppId
    {
      get
      {
        return _AppId;
      }
      set
      {
        __isset.AppId = true;
        this._AppId = value;
      }
    }

    public long TimestampMs
    {
      get
      {
        return _TimestampMs;
      }
      set
      {
        __isset.TimestampMs = true;
        this._TimestampMs = value;
      }
    }

    public List<string> MessageIds
    {
      get
      {
        return _MessageIds;
      }
      set
      {
        __isset.MessageIds = true;
        this._MessageIds = value;
      }
    }

    public long DeliveredWatermarkTimestampMs
    {
      get
      {
        return _DeliveredWatermarkTimestampMs;
      }
      set
      {
        __isset.DeliveredWatermarkTimestampMs = true;
        this._DeliveredWatermarkTimestampMs = value;
      }
    }


    public Isset __isset;
    #if !SILVERLIGHT
    [Serializable]
    #endif
    public struct Isset {
      public bool ThreadKey;
      public bool ActorFbId;
      public bool DeviceId;
      public bool AppId;
      public bool TimestampMs;
      public bool MessageIds;
      public bool DeliveredWatermarkTimestampMs;
    }

    public MNMessagesSyncDeltaDeliveryReceipt() {
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
              if (field.Type == TType.Struct) {
                ThreadKey = new MNMessagesSyncThreadKey();
                ThreadKey.Read(iprot);
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 2:
              if (field.Type == TType.I64) {
                ActorFbId = iprot.ReadI64();
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 3:
              if (field.Type == TType.String) {
                DeviceId = iprot.ReadString();
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 4:
              if (field.Type == TType.I64) {
                AppId = iprot.ReadI64();
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 5:
              if (field.Type == TType.I64) {
                TimestampMs = iprot.ReadI64();
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 6:
              if (field.Type == TType.List) {
                {
                  MessageIds = new List<string>();
                  TList _list107 = iprot.ReadListBegin();
                  for( int _i108 = 0; _i108 < _list107.Count; ++_i108)
                  {
                    string _elem109;
                    _elem109 = iprot.ReadString();
                    MessageIds.Add(_elem109);
                  }
                  iprot.ReadListEnd();
                }
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 7:
              if (field.Type == TType.I64) {
                DeliveredWatermarkTimestampMs = iprot.ReadI64();
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
        TStruct struc = new TStruct("MNMessagesSyncDeltaDeliveryReceipt");
        oprot.WriteStructBegin(struc);
        TField field = new TField();
        if (ThreadKey != null && __isset.ThreadKey) {
          field.Name = "ThreadKey";
          field.Type = TType.Struct;
          field.ID = 1;
          oprot.WriteFieldBegin(field);
          ThreadKey.Write(oprot);
          oprot.WriteFieldEnd();
        }
        if (__isset.ActorFbId) {
          field.Name = "ActorFbId";
          field.Type = TType.I64;
          field.ID = 2;
          oprot.WriteFieldBegin(field);
          oprot.WriteI64(ActorFbId);
          oprot.WriteFieldEnd();
        }
        if (DeviceId != null && __isset.DeviceId) {
          field.Name = "DeviceId";
          field.Type = TType.String;
          field.ID = 3;
          oprot.WriteFieldBegin(field);
          oprot.WriteString(DeviceId);
          oprot.WriteFieldEnd();
        }
        if (__isset.AppId) {
          field.Name = "AppId";
          field.Type = TType.I64;
          field.ID = 4;
          oprot.WriteFieldBegin(field);
          oprot.WriteI64(AppId);
          oprot.WriteFieldEnd();
        }
        if (__isset.TimestampMs) {
          field.Name = "TimestampMs";
          field.Type = TType.I64;
          field.ID = 5;
          oprot.WriteFieldBegin(field);
          oprot.WriteI64(TimestampMs);
          oprot.WriteFieldEnd();
        }
        if (MessageIds != null && __isset.MessageIds) {
          field.Name = "MessageIds";
          field.Type = TType.List;
          field.ID = 6;
          oprot.WriteFieldBegin(field);
          {
            oprot.WriteListBegin(new TList(TType.String, MessageIds.Count));
            foreach (string _iter110 in MessageIds)
            {
              oprot.WriteString(_iter110);
            }
            oprot.WriteListEnd();
          }
          oprot.WriteFieldEnd();
        }
        if (__isset.DeliveredWatermarkTimestampMs) {
          field.Name = "DeliveredWatermarkTimestampMs";
          field.Type = TType.I64;
          field.ID = 7;
          oprot.WriteFieldBegin(field);
          oprot.WriteI64(DeliveredWatermarkTimestampMs);
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
      StringBuilder __sb = new StringBuilder("MNMessagesSyncDeltaDeliveryReceipt(");
      bool __first = true;
      if (ThreadKey != null && __isset.ThreadKey) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("ThreadKey: ");
        __sb.Append(ThreadKey== null ? "<null>" : ThreadKey.ToString());
      }
      if (__isset.ActorFbId) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("ActorFbId: ");
        __sb.Append(ActorFbId);
      }
      if (DeviceId != null && __isset.DeviceId) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("DeviceId: ");
        __sb.Append(DeviceId);
      }
      if (__isset.AppId) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("AppId: ");
        __sb.Append(AppId);
      }
      if (__isset.TimestampMs) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("TimestampMs: ");
        __sb.Append(TimestampMs);
      }
      if (MessageIds != null && __isset.MessageIds) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("MessageIds: ");
        __sb.Append(MessageIds);
      }
      if (__isset.DeliveredWatermarkTimestampMs) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("DeliveredWatermarkTimestampMs: ");
        __sb.Append(DeliveredWatermarkTimestampMs);
      }
      __sb.Append(")");
      return __sb.ToString();
    }

  }

}