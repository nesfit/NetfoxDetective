using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Netfox.Web.DAL;
using Riganti.Utils.Infrastructure.Core;
using Riganti.Utils.Infrastructure.EntityFramework;

namespace Netfox.Web.BL.Queries
{
    public abstract class AppQueryBase<TResult> : EntityFrameworkQuery<TResult, NetfoxWebDbContext>
    {
        public AppQueryBase(IUnitOfWorkProvider unitOfWorkProvider) : base(unitOfWorkProvider)
        {
        }
    }
}
