using System.Data.Entity;
using System.Data.Entity.SqlServerCompact;

namespace EntityFramework.BulkInsert.Test.CodeFirst
{
    [DbConfigurationType(typeof (SqlCeConfig))]
    public class SqlCeContext : TestBaseContext
    {
        public SqlCeContext()
        {
        }

        public SqlCeContext(string cs)
            : base(cs)
        {

        }
    }

    public class SqlCeConfig : DbConfiguration
    {
        public SqlCeConfig()
        {
            this.SetProviderServices(
                SqlCeProviderServices.ProviderInvariantName,
                SqlCeProviderServices.Instance);
        }
    }
}
