using System.Collections.Generic;
using System.Linq;
using Netfox.Web.BL.Providers;

namespace Netfox.Web.BL.Queries
{
    public abstract class NetfoxQueryBase<TResult> 
    {
        protected readonly NetfoxUnitOfWorkProvider unitOfWorkProvider;

        public NetfoxQueryBase(NetfoxUnitOfWorkProvider unitOfWorkProvider) { this.unitOfWorkProvider = unitOfWorkProvider; }

        protected abstract IQueryable<TResult> GetQueryable();

        public virtual IList<TResult> Execute() { return this.GetQueryable().ToList<TResult>(); }

        public int TotalCount() { return this.GetQueryable().Count(); }

    }
}
