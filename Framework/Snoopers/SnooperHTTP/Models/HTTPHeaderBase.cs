// Copyright (c) 2017 Jan Pluskal, Miroslav Slivka, Vit Janecek
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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Castle.Core.Internal;
using Netfox.Core.Database.PersistableJsonSeializable;
using Netfox.Framework.ApplicationProtocolExport.PDUProviders;

namespace Netfox.SnooperHTTP.Models
{
    public enum MessageType
    {
        REQUEST,
        RESPONSE
    }
    [ComplexType]
    public abstract class HTTPHeaderBase
    {
        protected HTTPHeaderBase() { }
        public MessageType Type { get; protected set; }
        public string Version { get; protected set; }
        public bool IsPresent => this.Version != null;

        public PersistableJsonSerializableDictionaryStringListString Fields { get; set; } = new PersistableJsonSerializableDictionaryStringListString(StringComparer.OrdinalIgnoreCase);

        public abstract string StatusLine { get;  }
        
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach(var key in this.Fields.Keys)
            {
                sb.Append(key).Append(": ");
                foreach(var value in this.Fields[key]) { sb.Append(value).Append("; "); }
                sb.Append("\n");
            }
            return sb.ToString();
        }

        protected void Parse(PDUStreamReader reader)
        {
            var line = reader.ReadLine();
            while(!line.IsNullOrEmpty())
            {
                var fieldNameIndex = line.IndexOf(':');
                var fieldName = line.Substring(0, fieldNameIndex);
                var fieldValue = line.Substring(fieldNameIndex + 1, line.Length - fieldName.Length - 1).Trim();
                if(this.Fields.ContainsKey(fieldName)) { this.Fields[fieldName].Add(fieldValue); }
                else
                {
                    this.Fields[fieldName] = new List<string>
                    {
                        fieldValue
                    };
                }
                line = reader.ReadLine();
            }
        }
    }
}