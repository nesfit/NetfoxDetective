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
using System.Text;
using Thrift.Protocol;

namespace Netfox.Snoopers.SnooperMessenger.Protocol
{

  #if !SILVERLIGHT
  [Serializable]
  #endif
  public partial class MNMessagesSyncDeltaPagesManagerEvent : TBase
  {
    private MNMessagesSyncThreadKey _ThreadKey;
    private string _JsonBlob;

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

    public string JsonBlob
    {
      get
      {
        return _JsonBlob;
      }
      set
      {
        __isset.JsonBlob = true;
        this._JsonBlob = value;
      }
    }


    public Isset __isset;
    #if !SILVERLIGHT
    [Serializable]
    #endif
    public struct Isset {
      public bool ThreadKey;
      public bool JsonBlob;
    }

    public MNMessagesSyncDeltaPagesManagerEvent() {
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
              if (field.Type == TType.String) {
                JsonBlob = iprot.ReadString();
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
        TStruct struc = new TStruct("MNMessagesSyncDeltaPagesManagerEvent");
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
        if (JsonBlob != null && __isset.JsonBlob) {
          field.Name = "JsonBlob";
          field.Type = TType.String;
          field.ID = 2;
          oprot.WriteFieldBegin(field);
          oprot.WriteString(JsonBlob);
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
      StringBuilder __sb = new StringBuilder("MNMessagesSyncDeltaPagesManagerEvent(");
      bool __first = true;
      if (ThreadKey != null && __isset.ThreadKey) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("ThreadKey: ");
        __sb.Append(ThreadKey== null ? "<null>" : ThreadKey.ToString());
      }
      if (JsonBlob != null && __isset.JsonBlob) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("JsonBlob: ");
        __sb.Append(JsonBlob);
      }
      __sb.Append(")");
      return __sb.ToString();
    }

  }

}
