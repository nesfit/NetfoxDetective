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
 
    }
}