using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Netfox.Detective.Infrastructure
{
    public interface ISerializerFactory
    {
        XmlObjectSerializer Create(Type type);
        XmlObjectSerializer Create(Type type, IEnumerable<Type> knownTypes);
    }
}