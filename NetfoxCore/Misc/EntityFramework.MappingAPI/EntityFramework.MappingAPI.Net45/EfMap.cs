using EntityFramework.MappingAPI.Mappings;
using System;
using System.Collections.Concurrent;
using System.Data.Entity;
#if EF6
using System.Data.Entity.Infrastructure;
#endif

namespace EntityFramework.MappingAPI
{
    internal class EfMap
    {
        /// <summary>
        /// 
        /// </summary>
        private static readonly ConcurrentDictionary<string, DbMapping> Mappings = new ConcurrentDictionary<string, DbMapping>();

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IEntityMap<T> Get<T>(DbContext context)
        {
            return (IEntityMap<T>)Get(context)[typeof(T)];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IEntityMap Get(DbContext context, Type type)
        {
            return Get(context)[type];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IEntityMap Get(DbContext context, string typeFullName)
        {
            return Get(context)[typeFullName];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static DbMapping Get(DbContext context)
        {
            var cackeKey = context.GetType().FullName;
#if EF6
            var iDbModelCacheKeyProvider = context as IDbModelCacheKeyProvider;
            if (iDbModelCacheKeyProvider != null)
            {
                cackeKey = iDbModelCacheKeyProvider.CacheKey;
            }
#endif

            if (Mappings.ContainsKey(cackeKey))
            {
                return Mappings[cackeKey];
            }

            var mapping = new DbMapping(context);
            return Mappings.TryAdd(cackeKey, mapping) ? mapping : Mappings[cackeKey];
        }
    }
}
