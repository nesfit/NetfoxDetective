using System;
using System.Collections.Generic;
using System.Data.Entity;
using EntityFramework.Utilities.Mappings;

namespace EntityFramework.Utilities
{
    public class EfMappingFactory
    {
        private static Dictionary<Type, EfMapping> cache = new Dictionary<Type, EfMapping>();

        public static EfMapping GetMappingsForContext(DbContext context)
        {
            var type = context.GetType();
            EfMapping mapping;
            if (!cache.TryGetValue(type, out mapping))
            {
                mapping = new EfMapping(context);
                cache.Add(type, mapping);
            }

            return mapping;
        }
    }
}