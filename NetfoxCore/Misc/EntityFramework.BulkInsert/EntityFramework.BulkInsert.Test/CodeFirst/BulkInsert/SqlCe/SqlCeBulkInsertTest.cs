﻿
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
#if EF6 || EF61
using System.Data.Entity.Core.Common;
using System.Data.Entity.SqlServerCompact;
#endif
#if EF5
using System.Data.Common;
#endif
using EntityFramework.BulkInsert.SqlServerCe;

namespace EntityFramework.BulkInsert.Test.CodeFirst.BulkInsert.SqlCe
{
    public class SqlCeBulkInsertTest : BulkInsertTestBase<SqlCeBulkInsertProvider, SqlCeContext>
    {
        private bool _loaded = false;

        public override void Setup()
        {
#if EF6 || EF61
            if (!_loaded)
            {
                DbConfiguration.Loaded += (_, a) =>
                {
                    a.ReplaceService<DbProviderServices>((s, k) => SqlCeProviderServices.Instance);
                    a.ReplaceService<IDbConnectionFactory>(
                        (s, k) => new SqlCeConnectionFactory(SqlCeProviderServices.ProviderInvariantName));
                };
                _loaded = true;
            }
#endif
            base.Setup();
        }

        protected override SqlCeContext GetContext()
        {
            var context = new SqlCeContext("SqlCeContext");
            context.Database.CreateIfNotExists();
            return context;
        }

        public override void BulkInsertTableWithComputedColumns()
        {
            // not supported
        }

#if NET45
        public override void DbGeographyObject()
        {
            // not supported
        }
#endif
        
#if EF6 || EF61
        public override void Issue1344Test()
        {
            // not relavant
        }

        public override void Issue1369Test()
        {
            // not relavant
        }
#endif
        protected override string ProviderConnectionType
        {
            get { return "System.Data.SqlServerCe.SqlCeConnection"; }
        }
    }
}
