using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Netfox.Web.BL.DTO;
using Riganti.Utils.Infrastructure.Core;

namespace Netfox.Web.BL.Queries
{
    public class UserListQuery : AppQueryBase<UserDTO>, IFilteredQuery<UserDTO, UserFilterDTO>
    {
        public UserFilterDTO Filter { get; set; }

        public UserListQuery(IUnitOfWorkProvider unitOfWorkProvider) : base(unitOfWorkProvider)
        {
        }

        protected override IQueryable<UserDTO> GetQueryable()
        {
            return Context.Users
                .FilterOptionalString(u => u.Username + " " + u.Firstname + " " + u.Surname, Filter.SearchText, StringFilterMode.Contains)
                //.FilterOptional(u => u.IsEnable, Filter.IsEnable)
                .ProjectTo<UserDTO>();
        }
    }
}
 