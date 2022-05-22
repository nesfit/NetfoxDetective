using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Netfox.Web.BL.DTO;
using Riganti.Utils.Infrastructure.Core;

namespace Netfox.Web.BL.Queries
{
    public class InvestidationListQuery : AppQueryBase<InvestigationDTO>, IFilteredQuery<InvestigationDTO, InvestigationFilterDTO>
    {
        public InvestigationFilterDTO Filter { get; set; }

        public UserDTO User { get; set; } = null;

        public InvestidationListQuery(IUnitOfWorkProvider unitOfWorkProvider) : base(unitOfWorkProvider)
        {
        }

        protected override IQueryable<InvestigationDTO> GetQueryable()
        {
            var query = Context.Investigations.AsQueryable();

            if (this.User?.Role.Name == "Investigator")
            {
                query  = query.Where(i => (i.OwnerID == this.User.Id || i.UserInvestigations.Any(j => j.UserId == this.User.Id)));
            }

            return query
                .FilterOptionalString(i => i.Name + " " + " " + i.Description, Filter.SearchText, StringFilterMode.Contains)
                .ProjectTo<InvestigationDTO>();
        }
    }
}
 