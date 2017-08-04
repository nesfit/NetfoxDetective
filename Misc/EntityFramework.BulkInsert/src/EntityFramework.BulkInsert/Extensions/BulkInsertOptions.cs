using System.Data.SqlClient;

namespace EntityFramework.BulkInsert.Extensions
{
    public class BulkInsertOptions
    {
        /// <summary>
        /// 
        /// </summary>
        public int BatchSize { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public SqlBulkCopyOptions SqlBulkCopyOptions { get; set; }

        /// <summary>
        /// Number of the seconds for the operation to complete before it times out
        /// </summary>
        public int TimeOut { get; set; }

        /// <summary>
        /// Callback event handler. Event is fired after n (value from NotifyAfter) rows have been copied to table where.
        /// </summary>
        public SqlRowsCopiedEventHandler Callback { get; set; }

        /// <summary>
        /// Used with property Callback. Sets number of rows after callback is fired.
        /// </summary>
        public int NotifyAfter { get; set; }

#if !NET40
        /// <summary>
        /// 
        /// </summary>
        public bool EnableStreaming { get; set; }
#endif

        public BulkInsertOptions()
        {
            this.BatchSize = BulkInsertDefaults.BatchSize;
            this.SqlBulkCopyOptions = BulkInsertDefaults.SqlBulkCopyOptions;
            this.TimeOut = BulkInsertDefaults.TimeOut;
            this.NotifyAfter = BulkInsertDefaults.NotifyAfter;
        }
        /*

        /// <summary>
        /// Sets batch size
        /// </summary>
        /// <param name="batchSize"></param>
        /// <returns></returns>
        public BulkInsertOptions BatchSize(int batchSize)
        {
            BatchSizeValue = batchSize;
            return this;
        }

        /// <summary>
        /// Sets sql bulk copy timeout in seconds
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public BulkInsertOptions TimeOut(int timeout)
        {
            TimeOutValue = timeout;
            return this;
        }

        /// <summary>
        /// Sets SqlBulkCopy options
        /// </summary>
        /// <param name="sqlBulkCopyOptions"></param>
        /// <returns></returns>
        public BulkInsertOptions SqlBulkCopyOptions(SqlBulkCopyOptions sqlBulkCopyOptions)
        {
            SqlBulkCopyOptionsValue = sqlBulkCopyOptions;
            return this;
        }

        /// <summary>
        /// Sets callback method for sql bulk insert
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="interval">Numbers of rows processed before callback is invoked</param>
        /// <returns></returns>
        public BulkInsertOptions Callback(SqlRowsCopiedEventHandler callback, int interval)
        {
            CallbackMethod = callback;
            NotifyAfterValue = interval;

            return this;
        }

#if !NET40
        /// <summary>
        /// Sets batch size
        /// </summary>
        /// <param name="enableStreaming"></param>
        /// <returns></returns>
        public BulkInsertOptions EnableStreaming(bool enableStreaming)
        {
            EnableStreamingValue = enableStreaming;
            return this;
        }
#endif
        */
    }
}