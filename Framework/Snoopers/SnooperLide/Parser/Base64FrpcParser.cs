using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Netfox.Backend.Snoopers.SnooperLide.Parser
{
    public class Base64FrpcParser
    {
        private const int TYPE_MAGIC = 25;
        private const int TYPE_CALL = 13;
        private const int TYPE_RESPONSE = 14;
        private const int TYPE_FAULT = 15;

        private const int TYPE_INT = 1;
        private const int TYPE_BOOL = 2;
        private const int TYPE_DOUBLE = 3;
        private const int TYPE_STRING = 4;
        private const int TYPE_DATETIME = 5;
        private const int TYPE_BINARY = 6;
        private const int TYPE_INT8P = 7;
        private const int TYPE_INT8N = 8;
        private const int TYPE_STRUCT = 10;
        private const int TYPE_ARRAY = 11;
        private const int TYPE_NULL = 12;

        private byte[] _data = null;
        private int _pointer = 0;

        public byte[] Base64Atob(string data)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
            var asociatedAlphabet = new int[130];
            for (var j = 0; j < alphabet.Length; j++)
            {
                asociatedAlphabet[alphabet[j]] = j;
            }

            var output = new List<byte>();
            int chr1, chr2, chr3, enc1, enc2, enc3, enc4;
            data = Regex.Replace(data, @"\s+", "");
            int i = 0;

            while (i < data.Length)
            {
                enc1 = asociatedAlphabet[data[i]];
                enc2 = asociatedAlphabet[data[i + 1]];
                enc3 = asociatedAlphabet[data[i + 2]];
                enc4 = asociatedAlphabet[data[i + 3]];

                chr1 = (enc1 << 2) | (enc2 >> 4);
                chr2 = ((enc2 & 15) << 4) | (enc3 >> 2);
                chr3 = ((enc3 & 3) << 6) | enc4;

                output.Add((byte)chr1);
                if (enc3 != 64) { output.Add((byte)chr2); }
                if (enc4 != 64) { output.Add((byte)chr3); }

                i += 4;
            }

            return output.ToArray();
        }

        public string Parse(byte[] array)
        {
            this._data = array;
            string result = null;
            var magic1 = this.GetByte();
            var magic2 = this.GetByte();

            if (magic1 != 0xCA || magic2 != 0x11)
            {
                this._data = null;
                throw new FrpcException("Missing FRPC magic");
            }
            //Zahozeni nasledujicich dvou bytu z hlavicky:
            this.GetByte();
            this.GetByte();

            int first = this.getInt(1);
            int type = first >> 3;

            if (type == TYPE_FAULT)
            {
                throw new FrpcException("FRPC/" + this.ParseValue() + ": " + this.ParseValue());
            }

            switch (type)
            {
                case TYPE_RESPONSE:
                    result = this.ParseValue();
                    if (this._pointer < this._data.Length)
                    {
                        this._data = null;
                        throw new FrpcException("Garbage after FRPC _data");
                    }
                    break;

                case TYPE_CALL:
                    return null;

                default:
                    this._data = null;
                    throw new FrpcException("Unsupported FRPC type " + type);
            }
            this._data = null;
            return result;
        }

        private byte GetByte() 
        {
            if ((this._pointer + 1) > this._data.Length)
            {
                throw new IndexOutOfRangeException("Value of _pointer is out of range of array length.");
            }
            return this._data[this._pointer++];
        }

        private int getInt(int bytes)
        {
            int result = 0;
            int factor = 1;

            for (int i = 0; i < bytes; i++)
            {
                result += factor *this.GetByte();
                factor *= 256;
            }

            return result;
        }

        private string ParseValue()
        {
            int first = this.getInt(1);
            int type = first >> 3;

            int lengthBytes;
            int length;
            int members;
            string result;

            switch (type)
            {

                case TYPE_STRING:
                    lengthBytes = (first & 7) + 1;
                    length = this.getInt(lengthBytes);
                    result = this.DecodeUtf8(length);
                    result = Regex.Replace(result, "\"", "\\\"");
                    return "\"" + result + "\"";

                case TYPE_STRUCT:
                    result = "";
                    lengthBytes = (first & 7) + 1;
                    members = this.getInt(lengthBytes);
                    while (members != 0)
                    {
                        members--;
                        this.ParseMember(ref result);
                        if (members != 0)
                            result += ",";
                    }
                    return "{" + result + "}";

                case TYPE_ARRAY:
                    result = "";
                    lengthBytes = (first & 7) + 1;
                    members = this.getInt(lengthBytes);
                    while (members != 0)
                    {
                        members--;
                        result += this.ParseValue();
                        if (members != 0)
                            result += ",";
                    }
                    return "[" + result + "]";

                case TYPE_BOOL:
                    if ((first & 1) == 1)
                        return "true";
                    return "false";

                case TYPE_INT:
                    length = first & 7;
                    int max = Convert.ToInt32(Math.Pow(2, 8 * length));
                    int res = this.getInt(length);
                    if (res >= max / 2)
                        res -= max;
                    return res.ToString();

                case TYPE_DATETIME:
                    this.GetByte();
                    long ts = this.getInt(4);
                    for (var i = 0; i < 5; i++) this.GetByte();
                    DateTime date = this.TimeStampToDateTime(ts);
                    return "\"" + date.ToString("dd.MM.yyyy HH:mm:ss") + "\"";

                case TYPE_DOUBLE:
                    return this.GetDouble().ToString(CultureInfo.InvariantCulture);
                case TYPE_BINARY:
                    result = "";
                    lengthBytes = (first & 7) + 1;
                    length = this.getInt(lengthBytes);
                    while (length != 0)
                    {
                        length--;
                        result += this.GetByte().ToString();
                        if (length != 0)
                            result += ",";
                    }
                    return "[" + result + "]";

                case TYPE_INT8P:
                    length = (first & 7) + 1;
                    return this.getInt(length).ToString();

                case TYPE_INT8N:
                    length = (first & 7) + 1;
                    return (-this.getInt(length)).ToString();

                case TYPE_NULL:
                    return "null";

                default:
                    throw new FrpcException("Uknown FRPC type: " + type.ToString());
            }
        }

        private string ParseMember(ref string result)
        {
            int nameLength = this.getInt(1);
            string name = this.DecodeUtf8(nameLength);
            string value = this.ParseValue();

            result += "\"" + name + "\":" + value;
            return result;
        }

        private string DecodeUtf8(int length)
        {
            int remain = length;
            string result = "";

            if (length == 0)
                return result;

            var c = 0;
            var c1 = 0;
            var c2 = 0;
            byte[] data = this._data;
            int pointer = this._pointer;

            while (true)
            {
                remain--;
                c = data[pointer];
                pointer += 1;

                if (c < 128)
                {
                    result += Convert.ToChar(c);
                }
                else if ((c > 191) && (c < 224))
                {
                    c1 = data[pointer];
                    pointer += 1;
                    result += Convert.ToChar(((c & 31) << 6) | (c1 & 63));
                    remain -= 1;
                }
                else if (c < 240)
                {
                    c1 = data[pointer++];
                    c2 = data[pointer++];
                    result += Convert.ToChar(((c & 15) << 12) | ((c1 & 63) << 6) | (c2 & 63));
                    remain -= 2;
                }
                else if (c < 248)
                {
                    pointer += 3;
                    remain -= 3;
                }
                else if (c < 252)
                {
                    pointer += 4;
                    remain -= 4;
                }
                else
                {
                    pointer += 5;
                    remain -= 5;
                }

                if (remain <= 0)
                    break;
            }

            this._pointer = pointer + remain;
            return result;
        }

        private double GetDouble()
        {
            byte[] bytes = new byte[8];
            var index = 8;
            while (index != 0)
            {
                index--;
                bytes[index] = this.GetByte();
            }

            var sign = ((bytes[0] & 0x80) != 0 ? 1 : 0);
            var exponent = (bytes[0] & 127) << 4;
            exponent += bytes[1] >> 4;

            if (exponent == 0)
                return Math.Pow(-1, sign) * 0;

            int mantissa = 0;
            var byteIndex = 1;
            var bitIndex = 3;
            index = 1;
            do
            {
                var bitValue = ((bytes[byteIndex] & (1 << bitIndex)) != 0 ? 1 : 0);
                mantissa += bitValue * Convert.ToInt32(Math.Pow(2, -index));

                index++;
                bitIndex--;
                if (bitIndex < 0)
                {
                    bitIndex = 7;
                    byteIndex++;
                }
            } while (byteIndex < bytes.Length);

            if (exponent == 0x7ff)
            {
                if (mantissa != 0)
                {
                    return Double.NaN;
                }
                else {
                    return Math.Pow(-1, sign) * Double.PositiveInfinity;
                }
            }
            exponent -= (1 << 10) - 1;
            return Math.Pow(-1, sign) * Math.Pow(2, exponent) * (1 + mantissa);
        }
        private DateTime TimeStampToDateTime(double unixTimeStamp)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }

    [Serializable]
    public class FrpcException : Exception
    {
        public FrpcException(string message) : base(message) {}
    }
}