using System;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace EntityFramework.BulkInsert.Test.Issue1344
{
#if EF6 || EF61
    public class BaseXafConvention : Convention
    {
        public BaseXafConvention()
        {
            this.Properties<Guid>()
                .Where(p => p.Name == "Oid")
                .Configure(x => x.IsKey());

            this.Properties()
                .Where(p => p.Name == "GcRecord")
                .Configure(x => x.HasColumnName("GCRecord"));

            this.Properties<string>()
                .Configure(x => x.HasMaxLength(100));
        }
    }
#endif
}
