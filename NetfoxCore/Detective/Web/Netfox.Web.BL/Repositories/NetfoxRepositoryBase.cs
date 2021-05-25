using Netfox.Persistence;
using Riganti.Utils.Infrastructure.Core;
using Riganti.Utils.Infrastructure.EntityFramework;

namespace Netfox.Web.BL.Repositories
{
    public class NetfoxRepositoryBase<TEntity, TKey> : EntityFrameworkRepository<TEntity, TKey, NetfoxDbContext> where TEntity : class, IEntity<TKey>, new()
    {
        public NetfoxRepositoryBase(IEntityFrameworkUnitOfWorkProvider<NetfoxDbContext> unitOfWorkProvider, IDateTimeProvider dateTimeProvider) : base(unitOfWorkProvider, dateTimeProvider)
        {
        }

        public NetfoxRepositoryBase(IUnitOfWorkProvider unitOfWorkProvider, IDateTimeProvider dateTimeProvider) : base(unitOfWorkProvider, dateTimeProvider)
        {
        }
    }
}
