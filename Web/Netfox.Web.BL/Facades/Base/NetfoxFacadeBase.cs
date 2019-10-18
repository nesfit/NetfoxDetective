using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Netfox.Web.BL.Providers;
using UnitOfWork;
using UnitOfWork.BaseDataEntity;
using UnitOfWork.EF6Repository;

namespace Netfox.Web.BL.Facades.Base
{
    public abstract class NetfoxFacadeBase
    {
        protected NetfoxRepositoryProvider RepositoryProvider { get; set; } 
        protected NetfoxUnitOfWorkProvider UnitOfWorkProvider { get; set; }

        public NetfoxFacadeBase(NetfoxUnitOfWorkProvider unitofworkProvider, NetfoxRepositoryProvider repositoryProvider)
        {
            this.UnitOfWorkProvider = unitofworkProvider;
            this.RepositoryProvider = repositoryProvider;
        }

    }
}
