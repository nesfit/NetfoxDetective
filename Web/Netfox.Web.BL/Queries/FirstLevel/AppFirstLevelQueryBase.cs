using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Netfox.Web.DAL;
using Riganti.Utils.Infrastructure.Core;
using Riganti.Utils.Infrastructure.EntityFramework;

namespace Netfox.Web.BL.Queries.FirstLevel
{
    public class AppFirstLevelQueryBase<TResult> : EntityFrameworkFirstLevelQueryBase<TResult, NetfoxWebDbContext> where TResult : class
    {
        public AppFirstLevelQueryBase(IEntityFrameworkUnitOfWorkProvider<NetfoxWebDbContext> unitOfWorkProvider) : base(unitOfWorkProvider)
        {
        }

        public AppFirstLevelQueryBase(IUnitOfWorkProvider unitOfWorkProvider) : base(unitOfWorkProvider)
        {
        }
    }
}
