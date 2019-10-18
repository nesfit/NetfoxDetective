using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Netfox.Web.BL.DTO;
using Riganti.Utils.Infrastructure.Core;

namespace Netfox.Web.BL.Queries
{
    public class LastInvestidationListQuery : AppQueryBase<InvestigationDTO>
    {
       
        public UserDTO User { get; set; } = null;

        public LastInvestidationListQuery(IUnitOfWorkProvider unitOfWorkProvider) : base(unitOfWorkProvider)
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
 