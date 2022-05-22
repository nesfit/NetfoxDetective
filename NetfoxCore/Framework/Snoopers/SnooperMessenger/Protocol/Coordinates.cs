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
  public partial class Coordinates : TBase
  {
    private string _Latitude;
    private string _Longitude;
    private string _Accuracy;

    public string Latitude
    {
      get
      {
        return _Latitude;
      }
      set
      {
        __isset.Latitude = true;
        this._Latitude = value;
      }
    }

    public string Longitude
    {
      get
      {
        return _Longitude;
      }
      set
      {
        __isset.Longitude = true;
        this._Longitude = value;
      }
    }

    public string Accuracy
    {
      get
      {
        return _Accuracy;
      }
      set
      {
        __isset.Accuracy = true;
        this._Accuracy = value;
      }
    }


    public Isset __isset;
    #if !SILVERLIGHT
    [Serializable]
    #endif
    public struct Isset {
      public bool Latitude;
      public bool Longitude;
      public bool Accuracy;
    }

    public Coordinates() {
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
              if (field.Type == TType.String) {
                Latitude = iprot.ReadString();
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 2:
              if (field.Type == TType.String) {
                Longitude = iprot.ReadString();
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 3:
              if (field.Type == TType.String) {
                Accuracy = iprot.ReadString();
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
        TStruct struc = new TStruct("Coordinates");
        oprot.WriteStructBegin(struc);
        TField field = new TField();
        if (Latitude != null && __isset.Latitude) {
          field.Name = "Latitude";
          field.Type = TType.String;
          field.ID = 1;
          oprot.WriteFieldBegin(field);
          oprot.WriteString(Latitude);
          oprot.WriteFieldEnd();
        }
        if (Longitude != null && __isset.Longitude) {
          field.Name = "Longitude";
          field.Type = TType.String;
          field.ID = 2;
          oprot.WriteFieldBegin(field);
          oprot.WriteString(Longitude);
          oprot.WriteFieldEnd();
        }
        if (Accuracy != null && __isset.Accuracy) {
          field.Name = "Accuracy";
          field.Type = TType.String;
          field.ID = 3;
          oprot.WriteFieldBegin(field);
          oprot.WriteString(Accuracy);
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
      StringBuilder __sb = new StringBuilder("Coordinates(");
      bool __first = true;
      if (Latitude != null && __isset.Latitude) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("Latitude: ");
        __sb.Append(Latitude);
      }
      if (Longitude != null && __isset.Longitude) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("Longitude: ");
        __sb.Append(Longitude);
      }
      if (Accuracy != null && __isset.Accuracy) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("Accuracy: ");
        __sb.Append(Accuracy);
      }
      __sb.Append(")");
      return __sb.ToString();
    }

  }

}