using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFramework.InMemory
{
    internal class TestDbAsyncEnumerator<T> : IDbAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestDbAsyncEnumerator(IEnumerator<T> inner)
        {
            this._inner = inner;
        }

        public void Dispose()
        {
            this._inner.Dispose();
        }

        public Task<bool> MoveNextAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(this._inner.MoveNext());
        }

        public T Current => this._inner.Current;

        object IDbAsyncEnumerator.Current => this.Current;
    }
}