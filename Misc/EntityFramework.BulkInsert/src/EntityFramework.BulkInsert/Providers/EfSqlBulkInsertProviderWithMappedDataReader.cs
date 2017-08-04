using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using EntityFramework.BulkInsert.Helpers;

#if NET45
using Microsoft.SqlServer.Types;
using System.Threading.Tasks;
#endif

namespace EntityFramework.BulkInsert.Providers
{
    public class EfSqlBulkInsertProviderWithMappedDataReader : ProviderBase<SqlConnection, SqlTransaction>
    {
        /// <summary>
        /// Runs sql bulk insert using custom IDataReader
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <param name="transaction"></param>
        public override void Run<T>(IEnumerable<T> entities, SqlTransaction transaction)
        {
            var keepIdentity = (SqlBulkCopyOptions.KeepIdentity &this.Options.SqlBulkCopyOptions) > 0;
            using (var reader = new MappedDataReader<T>(entities, this))
            {
                using (var sqlBulkCopy = new SqlBulkCopy(transaction.Connection, this.Options.SqlBulkCopyOptions, transaction))
                {
                    sqlBulkCopy.BulkCopyTimeout = this.Options.TimeOut;
                    sqlBulkCopy.BatchSize = this.Options.BatchSize;
                    sqlBulkCopy.DestinationTableName = $"[{reader.SchemaName}].[{reader.TableName}]";
#if !NET40
                    sqlBulkCopy.EnableStreaming = this.Options.EnableStreaming;
#endif

                    sqlBulkCopy.NotifyAfter = this.Options.NotifyAfter;
                    if (this.Options.Callback != null)
                    {
                        sqlBulkCopy.SqlRowsCopied += this.Options.Callback;
                    }

                    foreach (var kvp in reader.Cols)
                    {
                        if (kvp.Value.IsIdentity && !keepIdentity)
                        {
                            continue;
                        }
                        sqlBulkCopy.ColumnMappings.Add(kvp.Value.ColumnName, kvp.Value.ColumnName);
                    }
                    sqlBulkCopy.WriteToServer(reader);
                   
                }
            }
        }

#if NET45

        /// <summary>
        /// Get sql grography object from well known text
        /// </summary>
        /// <param name="wkt">Well known text representation of the value</param>
        /// <param name="srid">The identifier associated with the coordinate system.</param>
        /// <returns></returns>
        public override object GetSqlGeography(string wkt, int srid)
        {
            var chars = new SqlChars(wkt);
            return SqlGeography.STGeomFromText(chars, srid);
        }

        /// <summary>
        /// Get sql geometry object from well known text
        /// </summary>
        /// <param name="wkt">Well known text representation of the value</param>
        /// <param name="srid">The identifier associated with the coordinate system.</param>
        /// <returns></returns>
        public override object GetSqlGeometry(string wkt, int srid)
        {
            var chars = new SqlChars(wkt);
            return SqlGeometry.STGeomFromText(chars, srid);
        }

        /// <summary>
        /// Runs sql bulk insert using custom IDataReader
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <param name="transaction"></param>
        public override async Task RunAsync<T>(IEnumerable<T> entities, SqlTransaction transaction)
        {
            var keepIdentity = (SqlBulkCopyOptions.KeepIdentity &this.Options.SqlBulkCopyOptions) > 0;
            using (var reader = new MappedDataReader<T>(entities, this))
            {
                using (var sqlBulkCopy = new SqlBulkCopy(transaction.Connection, this.Options.SqlBulkCopyOptions, transaction))
                {
                    sqlBulkCopy.BulkCopyTimeout = this.Options.TimeOut;
                    sqlBulkCopy.BatchSize = this.Options.BatchSize;
                    sqlBulkCopy.DestinationTableName = $"[{reader.SchemaName}].[{reader.TableName}]";
                    sqlBulkCopy.EnableStreaming = this.Options.EnableStreaming;


                    sqlBulkCopy.NotifyAfter = this.Options.NotifyAfter;
                    if (this.Options.Callback != null)
                    {
                        sqlBulkCopy.SqlRowsCopied += this.Options.Callback;
                    }

                    foreach (var kvp in reader.Cols)
                    {
                        if (kvp.Value.IsIdentity && !keepIdentity)
                        {
                            continue;
                        }
                        sqlBulkCopy.ColumnMappings.Add(kvp.Value.ColumnName, kvp.Value.ColumnName);
                    }
                   await sqlBulkCopy.WriteToServerAsync(reader);
                }
            }
        }

#endif

        /// <summary>
        /// Create new sql connection
        /// </summary>
        /// <returns></returns>
        protected override SqlConnection CreateConnection()
        {
            return new SqlConnection(this.ConnectionString);
        }
    }
}