using System;

namespace EntityFramework.BulkInsert.Exceptions
{
    public class EntityTypeNotFoundException : Exception
    {
        private readonly Type _type;

        public EntityTypeNotFoundException(Type type)
        {
            this._type = type;
        }

        public override string Message => $"Type '{this._type}' was not found in context";
    }
}