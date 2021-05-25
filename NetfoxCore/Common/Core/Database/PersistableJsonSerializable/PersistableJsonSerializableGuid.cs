using System;
using System.Collections.Generic;

namespace Netfox.Core.Database.PersistableJsonSerializable
{
    public class PersistableJsonSerializableGuid : PersistableJsonSerializable<Guid>
    {
        public PersistableJsonSerializableGuid()
        {
        }

        public PersistableJsonSerializableGuid(IEnumerable<Guid> collection) : base(collection)
        {
        }
    }
}