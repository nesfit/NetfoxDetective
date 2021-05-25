using System;
using System.Net;
using Newtonsoft.Json;

namespace Netfox.Core.Database.Wrappers
{
    public class IpEfConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((IPAddressEF)value).Address.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return new IPAddressEF(IPAddress.Parse(reader.ReadAsString()));
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IPAddressEF);
        }
    }
}