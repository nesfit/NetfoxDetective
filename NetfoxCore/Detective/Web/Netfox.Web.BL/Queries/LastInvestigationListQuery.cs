using System;
using System.Data.SqlTypes;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Netfox.Web.BL.DTO;
using Riganti.Utils.Infrastructure.Core;

namespace Netfox.Web.BL.Queries
{
    public class LastInvestigationListQuery : AppQueryBase<InvestigationDTO>
    {
       
        public UserDTO User { get; set; } = null;

        public LastInvestigationListQuery(IUnitOfWorkProvider unitOfWorkProvider) : base(unitOfWorkProvider)
        {
        }

        protected override IQueryable<InvestigationDTO> GetQueryable()
        {
            return Context.UserInvestigation
                .Where(i => i.UserId == User.Id && i.LastAccess != (DateTime)SqlDateTime.MinValue).OrderByDescending(i => i.LastAccess)
                .ProjectTo<InvestigationDTO>();
        }
    }
}
 