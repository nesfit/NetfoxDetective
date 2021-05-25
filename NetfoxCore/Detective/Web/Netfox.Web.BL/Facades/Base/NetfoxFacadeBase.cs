using Netfox.Web.BL.Providers;


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
