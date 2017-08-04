using System;

namespace EntityFramework.MappingAPI.Exceptions
{
    public class ProperyOverrideNotSupported : Exception
    {
        public Type ClassType { get; }
        public ProperyOverrideNotSupported(string message, Type type) : base(message)
        {
            this.ClassType = type;
        }
    }
}