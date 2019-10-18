using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Netfox.Web.DAL;
using Riganti.Utils.Infrastructure.Core;
using Riganti.Utils.Infrastructure.EntityFramework;

namespace Netfox.Web.BL.Repositories
{
    public class AppRepositoryBase<TEntity, TKey> : EntityFrameworkRepository<TEntity, TKey, NetfoxWebDbContext> where TEntity : class, IEntity<TKey>, new()
    {
        public AppRepositoryBase(IEntityFrameworkUnitOfWorkProvider<NetfoxWebDbContext> unitOfWorkProvider, IDateTimeProvider dateTimeProvider) : base(unitOfWorkProvider, dateTimeProvider)
        {
        }

        public AppRepositoryBase(IUnitOfWorkProvider unitOfWorkProvider, IDateTimeProvider dateTimeProvider) : base(unitOfWorkProvider, dateTimeProvider)
        {
        }
    }
}
