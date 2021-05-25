using System.Linq;
using Netfox.Web.DAL.Entities;
using Riganti.Utils.Infrastructure.Core;

namespace Netfox.Web.BL.Queries
{
    public class RoleListQuery : AppQueryBase<Role>
    {
        public string Username { get; set; }

        public string PasswordHash { get; set; }

        public RoleListQuery(IUnitOfWorkProvider unitOfWorkProvider) : base(unitOfWorkProvider) { }

        protected override IQueryable<Role> GetQueryable()
        {
            return this.Context.Roles.AsQueryable();
        }
    }

}