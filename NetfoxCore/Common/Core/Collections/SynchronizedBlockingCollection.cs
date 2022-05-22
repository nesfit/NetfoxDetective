using System.Collections.Concurrent;
using System.Threading;

namespace Netfox.Core.Collections
{
    public class SynchronizedBlockingCollection<T> : BlockingCollection<T>
    {
        public ManualResetEvent AddingCompleted { get; } = new ManualResetEvent(false);

        public new void CompleteAdding()
        {
            base.CompleteAdding();
            this.AddingCompleted.Set();
        }
    }
}