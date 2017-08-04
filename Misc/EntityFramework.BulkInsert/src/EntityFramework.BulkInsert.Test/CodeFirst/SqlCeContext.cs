#if EF6
using System.Data.Entity;
using System.Data.Entity.SqlServerCompact;
#endif

namespace EntityFramework.BulkInsert.Test.CodeFirst
{
#if EF6
    [DbConfigurationType(typeof (SqlCeConfig))]
#endif
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

#if EF6
    public class SqlCeConfig : DbConfiguration
    {
        public SqlCeConfig()
        {
            this.SetProviderServices(
                SqlCeProviderServices.ProviderInvariantName,
                SqlCeProviderServices.Instance);
        }
    }
#endif
}
