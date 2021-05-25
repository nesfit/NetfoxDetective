using System;

namespace EntityFramework.MappingAPI.Exceptions
{
    public class PropertyOverrideNotSupported : Exception
    {
        public Type ClassType { get; }
        public PropertyOverrideNotSupported(string message, Type type) : base(message)
        {
            this.ClassType = type;
        }
    }
}
