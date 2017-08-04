using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using EntityFramework.BulkInsert.Extensions;
#if NET45
using System.Threading.Tasks;
#endif

namespace EntityFramework.BulkInsert.Providers
{
    public abstract class ProviderBase<TConnection, TTransaction> : IEfBulkInsertProvider 
        where TConnection : IDbConnection
        where TTransaction : IDbTransaction
    {
        /// <summary>
        /// Current DbContext
        /// </summary>
        public DbContext Context { get; private set; }

        public BulkInsertOptions Options { get; set; }

        /// <summary>
        /// Connection string which current dbcontext is using
        /// </summary>
        protected virtual string ConnectionString => (string)this.DbConnection.GetPrivateFieldValue("_connectionString");

        protected virtual IDbConnection DbConnection => this.Context.Database.Connection;

#if NET45

        /// <summary>
        /// Get sql grography object from well known text
        /// </summary>
        /// <param name="wkt">Well known text representation of the value</param>
        /// <param name="srid">The identifier associated with the coordinate system.</param>
        /// <returns></returns>
        public abstract object GetSqlGeography(string wkt, int srid);

        /// <summary>
        /// Get sql geometry object from well known text
        /// </summary>
        /// <param name="wkt">Well known text representation of the value</param>
        /// <param name="srid">The identifier associated with the coordinate system.</param>
        /// <returns></returns>
        public abstract object GetSqlGeometry(string wkt, int srid);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <param name="transaction"></param>
        public Task RunAsync<T>(IEnumerable<T> entities, IDbTransaction transaction)
        {
            return this.RunAsync(entities, (TTransaction)transaction);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        public virtual async Task RunAsync<T>(IEnumerable<T> entities)
        {
            var enumerable = entities as T[] ?? entities.ToArray();
            while (true)
            using (var dbConnection = this.GetConnection())
            {
                dbConnection.Open();

                using (var transaction = dbConnection.BeginTransaction())
                {
                    try
                    {
                        await this.RunAsync(enumerable, transaction);
                        transaction.Commit();
                        return;
                    }
                    catch (Exception)
                    {
                        if (transaction.Connection != null)
                        {
                            transaction.Rollback();
                        }
                    }
                }
            }
        }

#endif

        /// <summary>
        /// Sets DbContext for bulk insert to use
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public IEfBulkInsertProvider SetContext(DbContext context)
        {
            this.Context = context;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IDbConnection GetConnection()
        {
            return this.CreateConnection();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected abstract TConnection CreateConnection();

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <param name="transaction"></param>
        public void Run<T>(IEnumerable<T> entities, IDbTransaction transaction)
        {
            this.Run(entities, (TTransaction)transaction);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        public virtual void Run<T>(IEnumerable<T> entities)
        {
            var enumerable = entities as T[] ?? entities.ToArray();
            while (true)
                using (var dbConnection = this.GetConnection())
                {
                    dbConnection.Open();

                    using (var transaction = dbConnection.BeginTransaction())
                    {
                        try
                        {
                            this.Run(enumerable, transaction);
                            transaction.Commit();
                            return;
                        }
                        catch (Exception)
                        {
                            if (transaction.Connection != null)
                            {
                                transaction.Rollback();
                            }
                        }
                    }
                }
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <param name="transaction"></param>
        public abstract void Run<T>(IEnumerable<T> entities, TTransaction transaction);

#if NET45
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities">The entities.</param>
        /// <param name="transaction">The transaction.</param>
        public abstract Task RunAsync<T>(IEnumerable<T> entities, TTransaction transaction);
#endif

    }
}