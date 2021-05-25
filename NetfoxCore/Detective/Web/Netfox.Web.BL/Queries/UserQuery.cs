using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Netfox.Web.BL.DTO;
using Riganti.Utils.Infrastructure.Core;

namespace Netfox.Web.BL.Queries
{
    public class UserQuery : AppQueryBase<UserDTO>
    {
        public string Username { get; set; }

        public UserQuery(IUnitOfWorkProvider unitOfWorkProvider) : base(unitOfWorkProvider)
        {
        }

        protected override IQueryable<UserDTO> GetQueryable()
        {
            return Context.Users.Where(u => u.Username == this.Username).ProjectTo<UserDTO>();
        }
    }
}
 