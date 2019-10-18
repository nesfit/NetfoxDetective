using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Netfox.Web.BL.DTO;
using Netfox.Web.DAL.Entities;
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
 