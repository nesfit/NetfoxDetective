using System;

namespace Netfox.Core.Database
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PersistentAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class PersistentNonInheritedAttribute : PersistentAttribute
    {
    }
}