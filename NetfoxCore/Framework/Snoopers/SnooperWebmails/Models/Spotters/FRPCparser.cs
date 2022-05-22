// Copyright (c) 2017 Jan Pluskal, Miroslav Slivka
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
using System.Text;

namespace Netfox.Snoopers.SnooperWebmails.Models.Spotters
{
    public class FRPCparser
    {
        public enum ItemType
        {
            MethodCall,
            MethodReponse,
            FaultReponse,
            Integer8Positive,
            Integer8Negative,
            Array,
            Struct,
            String,
            Boolean,
            DateTime,
            Null,
            Binary
        }

        public static bool Parse(byte[] data, out List<IFRPCItem> items)
        {
            items = null;

            if(data.Length < 4) { return false; }

            int majorVersion;
            int minorVersion;

            if(data[0] != 0xCA || data[1] != 0x11) { return false; }

            majorVersion = data[2];
            minorVersion = data[3];

            int processedLen;
            return ParseTypes(data, 4, out items, out processedLen);
        }

        private static bool ParseDataType(byte[] data, int offset, out IFRPCItem item, out int processedLen)
        {
            var pos = offset;

            var parseOk = true;

            processedLen = 0;
            item = null;

            if(pos >= data.Length) { return false; }

            int d = data[pos];

            var typeCode = ((data[pos] >> 3));
            var typeLen = ((data[pos]&0x07));

            if(typeCode == 0x0D)
            {
                int nameSize = data[pos + 1];
                var name = Encoding.ASCII.GetString(data, pos + 2, nameSize);
                var methodCall = new FRPCMethodCall(name);
                pos += 2 + nameSize;
                List<IFRPCItem> perameters;
                int perametersLen;
                ParseTypes(data, pos, out perameters, out perametersLen);
                methodCall.Parameters = perameters;
                item = methodCall;
                pos += perametersLen;
            }
            else if(typeCode == 0x0E)
            {
                var methodRespone = new FRPCMethodRespone();
                List<IFRPCItem> responeItems;
                int dataLen;
                ParseTypes(data, pos + 1, out responeItems, out dataLen);
                methodRespone.Data = responeItems;
                pos += dataLen + 1;
                item = methodRespone;
            }
            else if(typeCode == 0x02)
            {
                item = new FRPCBoolean(typeLen == 0x01);
                pos += 1;
            }
            else if(typeCode == 0x04)
            {
                var valSize = 0;
                for(var i = 0; i < typeLen + 1; i++) { valSize |= data[pos + i + 1] << (i*8); }

                var value = Encoding.UTF8.GetString(data, pos + typeLen + 2, valSize);
                item = new FRPCString(value);
                pos += 2 + typeLen + valSize;
            }
            else if(typeCode == 0x07 || typeCode == 0x08)
            {
                var iVal = 0;

                for(var i = 0; i < typeLen + 1; i++)
                {
                    //iVal = (iVal << 8) | data[pos + i + 1];
                    iVal |= data[pos + i + 1] << (i*8);
                }

                if(typeCode == 0x08) { iVal = -iVal; }

                item = new FRPCInteger8(iVal);
                pos += 1 + typeLen + 1;
            }
            else if(typeCode == 0x01)
            {
                var iVal = 0;

                for(var i = 0; i < typeLen; i++) { iVal |= data[pos + i] << (i*8); }

                item = new FRPCInteger8(iVal);
                pos += 1 + typeLen;
            }
            else if(typeCode == 0x0B)
            {
                var itemsCount = 0;
                for(var i = 0; i < typeLen + 1; i++)
                {
                    //itemsCount = (itemsCount << 8) | data[pos + i + 1];
                    itemsCount |= data[pos + i + 1] << (i*8);
                }

                var newArray = new FRPCArray();
                var arrayItems = new List<IFRPCItem>();

                pos += 1 + typeLen + 1;

                for(var i = 0; parseOk && i < itemsCount; i++)
                {
                    int itemLen;
                    IFRPCItem arrayItem;
                    parseOk = ParseDataType(data, pos, out arrayItem, out itemLen);
                    arrayItems.Add(arrayItem);
                    pos += itemLen;
                }

                newArray.Items = arrayItems;
                item = newArray;
            }
            else if(typeCode == 0x0A)
            {
                var itemsCount = 0;
                for(var i = 0; i < typeLen + 1; i++)
                {
                    //  itemsCount = (itemsCount << 8) | data[pos + i + 1];
                    itemsCount |= data[pos + i + 1] << (i*8);
                }

                var newStruct = new FRPCStruct();
                var structItems = new List<KeyValuePair<string, IFRPCItem>>();

                pos += 1 + typeLen + 1;

                for(var i = 0; i < itemsCount; i++)
                {
                    int nameSize = data[pos];
                    var name = Encoding.ASCII.GetString(data, pos + 1, nameSize);
                    int itemLen;
                    IFRPCItem arrayItem;
                    parseOk = ParseDataType(data, pos + 1 + nameSize, out arrayItem, out itemLen);
                    structItems.Add(new KeyValuePair<string, IFRPCItem>(name, arrayItem));
                    pos += 1 + nameSize + itemLen;
                }

                newStruct.Items = structItems;
                item = newStruct;
            }
            else if(typeCode == 0x05)
            {
                // datetime skip for now

                int zone = data[pos + 1];
                var timeStamp = data[pos + 2]|data[pos + 3] << 8|data[pos + 4] << 16|data[pos + 5] << 24;

                var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                dateTime = dateTime.AddSeconds(timeStamp).ToLocalTime();

                pos += 11;

                item = new FRPCDateTime(dateTime);
            }
            else if(typeCode == 0x0C)
            {
                pos += 1;

                item = new FRPCNull();
            }
            else if(typeCode == 0x06)
            {
                var bytesCount = 0;
                for(var i = 0; i < typeLen + 1; i++)
                {
                    // bytesCount = (bytesCount << 8) | data[pos + i + 1];
                    bytesCount |= data[pos + i + 1] << (i*8);
                }

                var binaryData = new byte[bytesCount];
                Array.Copy(data, pos + 1 + typeLen, binaryData, 0, bytesCount);

                var binaryItem = new FRPCBinary
                {
                    Byte = binaryData
                };

                pos += 1 + typeLen + bytesCount;

                item = binaryItem;
            }
            else
            { parseOk = false; }

            processedLen = pos - offset;

            return parseOk;
        }

        private static bool ParseTypes(byte[] data, int offset, out List<IFRPCItem> items, out int processedLen)
        {
            items = new List<IFRPCItem>();

            var parseOk = true;
            var pos = offset;

            while(parseOk && pos < data.Length)
            {
                IFRPCItem item;
                int len;
                parseOk = ParseDataType(data, pos, out item, out len);
                pos += len;

                items.Add(item);
            }

            processedLen = pos - offset;
            return parseOk;
        }

        public class FRPCArray : IFRPCItem
        {
            public FRPCArray() { this.Type = ItemType.Array; }
            public List<IFRPCItem> Items { get; set; }
            public ItemType Type { get; set; }
        }

        public class FRPCBinary : IFRPCItem
        {
            public FRPCBinary() { this.Type = ItemType.Binary; }
            public byte[] Byte { get; set; }
            public ItemType Type { get; set; }
        }

        public class FRPCBoolean : IFRPCItem
        {
            public FRPCBoolean(bool value)
            {
                this.Value = value;
                this.Type = ItemType.Boolean;
            }

            public bool Value { get; set; }
            public ItemType Type { get; set; }
        }

        public class FRPCDateTime : IFRPCItem
        {
            public FRPCDateTime(DateTime dateTime)
            {
                this.DateTime = dateTime;
                this.Type = ItemType.DateTime;
            }

            public DateTime DateTime { get; set; }
            public ItemType Type { get; set; }
        }

        public class FRPCInteger8 : IFRPCItem
        {
            public FRPCInteger8(int value)
            {
                this.Value = value;
                this.Type = ItemType.Integer8Positive;
            }

            public int Value { get; set; }
            public ItemType Type { get; set; }
        }

        public class FRPCMethodCall : IFRPCItem
        {
            public FRPCMethodCall(string name)
            {
                this.Name = name;
                this.Type = ItemType.MethodCall;
            }

            public string Name { get; set; }
            public List<IFRPCItem> Parameters { get; set; }
            public ItemType Type { get; set; }
        }

        public class FRPCMethodRespone : IFRPCItem
        {
            public FRPCMethodRespone() { this.Type = ItemType.MethodReponse; }
            public List<IFRPCItem> Data { get; set; }
            public ItemType Type { get; set; }
        }

        public class FRPCNull : IFRPCItem
        {
            public FRPCNull() { this.Type = ItemType.Null; }
            public ItemType Type { get; set; }
        }

        public class FRPCString : IFRPCItem
        {
            public FRPCString(string value)
            {
                this.Value = value;
                this.Type = ItemType.String;
            }

            public string Value { get; set; }
            public ItemType Type { get; set; }
        }

        public class FRPCStruct : IFRPCItem
        {
            public FRPCStruct() { this.Type = ItemType.Struct; }
            public List<KeyValuePair<string, IFRPCItem>> Items { get; set; }
            public ItemType Type { get; set; }
        }

        public interface IFRPCItem
        {
            ItemType Type { get; set; }
        }
    }
}