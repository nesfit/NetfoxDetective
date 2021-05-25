using System;
using System.Collections.Concurrent;
using System.IO;

namespace Netfox.Framework.Models.PmLib
{
    /// <remarks>
    ///     Pool of binary readers for concurrent data retrieval
    /// </remarks>
    public class BinaryReadersPool : IDisposable
    {
        private readonly FileInfo _fileInfo;
        private readonly ConcurrentBag<BinaryReader> _readersAllOpened;
        private readonly ConcurrentBag<BinaryReader> _readersPool;

        public BinaryReadersPool(FileInfo fileInfo)
        {
            this._readersPool = new ConcurrentBag<BinaryReader>();
            this._fileInfo = fileInfo;
            this._readersAllOpened = new ConcurrentBag<BinaryReader>();
        }

        public BinaryReader GetReader()
        {
            BinaryReader reader;

            if (this._readersPool.TryTake(out reader))
            {
                return reader;
            }

            reader = new BinaryReader(new FileStream(this._fileInfo.FullName, FileMode.Open, FileAccess.Read,
                FileShare.Read));
            this._readersAllOpened.Add(reader);
            return reader;
        }

        public void PutReader(BinaryReader item) => this._readersPool.Add(item);

        #region Implementation of IDisposable

        bool _disposed = false;

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (this._disposed)
                return;

            if (disposing)
            {
                foreach (var reader in this._readersAllOpened)
                {
                    reader.Close();
                }

                // Free any other managed objects here.
            }

            // Free any unmanaged objects here.
            this._disposed = true;
        }

        #endregion
    }
}