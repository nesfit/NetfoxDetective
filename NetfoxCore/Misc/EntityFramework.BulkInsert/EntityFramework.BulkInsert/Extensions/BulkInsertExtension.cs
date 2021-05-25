using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using EntityFramework.BulkInsert.Providers;

namespace EntityFramework.BulkInsert.Extensions
{
    public static class BulkInsertExtension
    {
#if NET45 || NET5_0_OR_GREATER
        public static Task BulkInsertAsync<T>(this DbContext context, IEnumerable<T> entities, BulkInsertOptions options)
        {
            IEfBulkInsertProvider bulkInsert = ProviderFactory.Get(context);
            bulkInsert.Options = options;
            return bulkInsert.RunAsync(entities);
        }


        /// <returns></returns>
        public static Task BulkInsertAsync<T>(this DbContext context, IEnumerable<T> entities, int? batchSize = null)
        {
            return context.BulkInsertAsync(entities, SqlBulkCopyOptions.Default, batchSize);
        }

        public static Task BulkInsertAsync<T>(this DbContext context, IEnumerable<T> entities, SqlBulkCopyOptions sqlBulkCopyOptions, int? batchSize
 = null)
        {
            var options = new BulkInsertOptions { SqlBulkCopyOptions = sqlBulkCopyOptions };
            if (batchSize.HasValue)
            {
                options.BatchSize = batchSize.Value;
            }
            return context.BulkInsertAsync(entities, options);
        }


        public static Task BulkInsertAsync<T>(this DbContext context, IEnumerable<T> entities, IDbTransaction transaction, SqlBulkCopyOptions sqlBulkCopyOptions
 = SqlBulkCopyOptions.Default, int? batchSize = null)
        {
            var options = new BulkInsertOptions { SqlBulkCopyOptions = sqlBulkCopyOptions };
            if (batchSize.HasValue)
            {
                options.BatchSize = batchSize.Value;
            }
            return context.BulkInsertAsync(entities, options);
        }
#endif
        
        public static void BulkInsert<T>(this DbContext context, IEnumerable<T> entities, BulkInsertOptions options)
        {
            var bulkInsert = ProviderFactory.Get(context);
            bulkInsert.Options = options;
            bulkInsert.Run(entities);
        }

        public static void BulkInsert<T>(this DbContext context, IEnumerable<T> entities, int? batchSize = null)
        {
            context.BulkInsert(entities, SqlBulkCopyOptions.Default, batchSize);
        }


        public static void BulkInsert<T>(this DbContext context, IEnumerable<T> entities,
            SqlBulkCopyOptions sqlBulkCopyOptions, int? batchSize = null)
        {
            var options = new BulkInsertOptions {SqlBulkCopyOptions = sqlBulkCopyOptions};
            if (batchSize.HasValue)
            {
                options.BatchSize = batchSize.Value;
            }

            context.BulkInsert(entities, options);
        }

        public static void BulkInsert<T>(this DbContext context, IEnumerable<T> entities, IDbTransaction transaction,
            SqlBulkCopyOptions sqlBulkCopyOptions = SqlBulkCopyOptions.Default, int? batchSize = null)
        {
            var options = new BulkInsertOptions {SqlBulkCopyOptions = sqlBulkCopyOptions};
            if (batchSize.HasValue)
            {
                options.BatchSize = batchSize.Value;
            }

            context.BulkInsert(entities, options);
        }
#if NET45 || NET5_0_OR_GREATER
        
        public static async Task BulkInsertBuffered<T>(this DbContext context, IEnumerable<T> entities, BulkInsertOptions options)
        {
            var bulkInsert = ProviderFactory.Get(context);
            bulkInsert.Options = options;
            var mutualExclusionLock = new object();

            var t1 =
 Task.Run(async () => await BulkInsertBufferedBody<T>(bulkInsert, entities, options, mutualExclusionLock));
            var t2 =
 Task.Run(async () => await BulkInsertBufferedBody<T>(bulkInsert, entities, options, mutualExclusionLock));
           
            await Task.WhenAll(t1,t2);
        }

        public static async Task BulkInsertBufferedBody<T>(IEfBulkInsertProvider bulkInsert,IEnumerable<T> entities, BulkInsertOptions options, object mutualExclusionLock)
        {
            while (true)
            {
                var buffer = new List<T>();
                lock (mutualExclusionLock)
                {
                    foreach (var item in entities)
                    {
                        buffer.Add(item);
                        if (buffer.Count == options.BatchSize) break;
                    }
                }

                if(!buffer.Any())return;
                await BulkInsertAsync(bulkInsert.Context, buffer, options);
            }
        }

        public static async Task BulkInsertBuffered<T>(this DbContext context, IEnumerable<T> entities, int? batchSize =
 null)
        {
           await context.BulkInsertBuffered(entities, SqlBulkCopyOptions.Default, batchSize);
        }


        public static async Task BulkInsertBuffered<T>(this DbContext context, IEnumerable<T> entities, SqlBulkCopyOptions sqlBulkCopyOptions, int? batchSize
 = null)
        {

            var options = new BulkInsertOptions { SqlBulkCopyOptions = sqlBulkCopyOptions };
            if (batchSize.HasValue)
            {
                options.BatchSize = batchSize.Value;
            }
            await context.BulkInsertBuffered(entities, options);
        }

        public static async Task BulkInsertBuffered<T>(this DbContext context, IEnumerable<T> entities, IDbTransaction transaction, SqlBulkCopyOptions sqlBulkCopyOptions
 = SqlBulkCopyOptions.Default, int? batchSize = null)
        {
            var options = new BulkInsertOptions { SqlBulkCopyOptions = sqlBulkCopyOptions };
            if (batchSize.HasValue)
            {
                options.BatchSize = batchSize.Value;
            }
            await context.BulkInsertBuffered(entities, options);
        }
#endif
    }
}