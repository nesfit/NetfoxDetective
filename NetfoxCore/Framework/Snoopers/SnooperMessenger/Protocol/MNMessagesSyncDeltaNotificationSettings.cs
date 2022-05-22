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
  public partial class MNMessagesSyncDeltaNotificationSettings : TBase
  {
    private MNMessagesSyncThreadKey _ThreadKey;
    private List<MNMessagesSyncNotificationDoNotDisturbRange> _DoNotDisturbRanges;

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

    public List<MNMessagesSyncNotificationDoNotDisturbRange> DoNotDisturbRanges
    {
      get
      {
        return _DoNotDisturbRanges;
      }
      set
      {
        __isset.DoNotDisturbRanges = true;
        this._DoNotDisturbRanges = value;
      }
    }


    public Isset __isset;
    #if !SILVERLIGHT
    [Serializable]
    #endif
    public struct Isset {
      public bool ThreadKey;
      public bool DoNotDisturbRanges;
    }

    public MNMessagesSyncDeltaNotificationSettings() {
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
              if (field.Type == TType.List) {
                {
                  DoNotDisturbRanges = new List<MNMessagesSyncNotificationDoNotDisturbRange>();
                  TList _list116 = iprot.ReadListBegin();
                  for( int _i117 = 0; _i117 < _list116.Count; ++_i117)
                  {
                    MNMessagesSyncNotificationDoNotDisturbRange _elem118;
                    _elem118 = new MNMessagesSyncNotificationDoNotDisturbRange();
                    _elem118.Read(iprot);
                    DoNotDisturbRanges.Add(_elem118);
                  }
                  iprot.ReadListEnd();
                }
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
        TStruct struc = new TStruct("MNMessagesSyncDeltaNotificationSettings");
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
        if (DoNotDisturbRanges != null && __isset.DoNotDisturbRanges) {
          field.Name = "DoNotDisturbRanges";
          field.Type = TType.List;
          field.ID = 2;
          oprot.WriteFieldBegin(field);
          {
            oprot.WriteListBegin(new TList(TType.Struct, DoNotDisturbRanges.Count));
            foreach (MNMessagesSyncNotificationDoNotDisturbRange _iter119 in DoNotDisturbRanges)
            {
              _iter119.Write(oprot);
            }
            oprot.WriteListEnd();
          }
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
      StringBuilder __sb = new StringBuilder("MNMessagesSyncDeltaNotificationSettings(");
      bool __first = true;
      if (ThreadKey != null && __isset.ThreadKey) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("ThreadKey: ");
        __sb.Append(ThreadKey== null ? "<null>" : ThreadKey.ToString());
      }
      if (DoNotDisturbRanges != null && __isset.DoNotDisturbRanges) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("DoNotDisturbRanges: ");
        __sb.Append(DoNotDisturbRanges);
      }
      __sb.Append(")");
      return __sb.ToString();
    }

  }

}