#if NET45 || NET5_0_OR_GREATER
#if EF6 || EF61
using System.Data.Entity.Spatial;
#endif
#if EF5
using System.Data.Spatial;
#endif
#endif

namespace EntityFramework.BulkInsert.Test.CodeFirst.Domain
{
    public class PinPoint : Entity
    {
        public string Name { get; set; }

#if NET45 || NET5_0_OR_GREATER
        public DbGeography Coordinates { get; set; }
#endif
    }
}