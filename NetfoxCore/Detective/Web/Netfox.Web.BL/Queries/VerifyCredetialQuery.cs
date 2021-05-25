using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Netfox.Web.BL.DTO;
using Riganti.Utils.Infrastructure.Core;

namespace Netfox.Web.BL.Queries
{
    public class VerifyCredetialQuery : AppQueryBase<UserDTO>
    {
        public string Username { get; set; }

        public string PasswordHash { get; set; }

        public VerifyCredetialQuery(IUnitOfWorkProvider unitOfWorkProvider) : base(unitOfWorkProvider) { }

        protected override IQueryable<UserDTO> GetQueryable()
        {
            return this.Context.Users
            .Where(u => u.Username == Username && u.Password == PasswordHash)
            .ProjectTo<UserDTO>();
        }
    }

}