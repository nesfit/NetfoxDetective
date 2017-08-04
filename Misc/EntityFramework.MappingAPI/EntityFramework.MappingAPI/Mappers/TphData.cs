using System.Collections.Generic;


#if EF6
    using System.Data.Entity.Core.Metadata.Edm;
#else
    using System.Data.Metadata.Edm;
#endif

namespace EntityFramework.MappingAPI.Mappers
{
    internal class TphData
    {
        public EdmMember[] Properties { get; set; }
        public NavigationProperty[] NavProperties { get; set; }

        public Dictionary<string, object> Discriminators = new Dictionary<string, object>();
    }
}