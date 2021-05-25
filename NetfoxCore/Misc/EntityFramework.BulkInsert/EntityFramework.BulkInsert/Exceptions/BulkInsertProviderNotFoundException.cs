using System;

namespace EntityFramework.BulkInsert.Exceptions
{
    public class BulkInsertProviderNotFoundException : Exception
    {
        private readonly string _connectionType;

        public BulkInsertProviderNotFoundException(string connectionType)
        {
            this._connectionType = connectionType;
        }

        public override string Message =>
            $"BulkInsertProvider not found for '{this._connectionType}.\nTo register new provider use EntityFramework.BulkInsert.ProviderFactory.Register() method'";
    }
}