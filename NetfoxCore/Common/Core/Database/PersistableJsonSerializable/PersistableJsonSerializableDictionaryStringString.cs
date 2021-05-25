using System.Collections.Generic;

namespace Netfox.Core.Database.PersistableJsonSerializable
{
    public class
        PersistableJsonSerializableDictionaryStringString : PersistableJsonSerializableDictionary<string, string>
    {
        public PersistableJsonSerializableDictionaryStringString()
        {
        }

        public PersistableJsonSerializableDictionaryStringString(Dictionary<string, string> initDictionary) : base(
            initDictionary)
        {
        }
    }
}