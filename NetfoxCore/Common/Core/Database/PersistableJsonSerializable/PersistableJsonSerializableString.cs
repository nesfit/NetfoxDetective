using System.Collections.Generic;

namespace Netfox.Core.Database.PersistableJsonSerializable
{
    public class PersistableJsonSerializableString : PersistableJsonSerializable<string>
    {
        public PersistableJsonSerializableString() : base()
        {
        }

        public PersistableJsonSerializableString(IEnumerable<string> collection) : base(collection)
        {
        }
    }
}