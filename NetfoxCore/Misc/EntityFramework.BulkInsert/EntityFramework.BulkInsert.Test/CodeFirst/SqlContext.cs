
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using EntityFramework.BulkInsert.Test.CodeFirst.Domain;

namespace EntityFramework.BulkInsert.Test.CodeFirst
{
    [DbConfigurationType(typeof(SqlContextConfig))]
    public class SqlContext : TestBaseContext
    {

    }

    public class SqlContextConfig : DbConfiguration
    {
        public SqlContextConfig()
        {
            SetExecutionStrategy("System.Data.SqlClient", () => new DefaultExecutionStrategy());
        }
    }
}
