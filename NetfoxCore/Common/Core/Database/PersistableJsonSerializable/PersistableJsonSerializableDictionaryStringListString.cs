using System;
using System.Collections.Generic;

namespace Netfox.Core.Database.PersistableJsonSerializable
{
    public class
        PersistableJsonSerializableDictionaryStringListString : PersistableJsonSerializableDictionary<string,
            List<string>>
    {
        public PersistableJsonSerializableDictionaryStringListString()
        {
        }

        public PersistableJsonSerializableDictionaryStringListString(StringComparer ordinalIgnoreCase) : base(
            ordinalIgnoreCase)
        {
        }
    }
}