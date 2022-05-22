using System;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Netfox.Web.BL.DTO;
using Riganti.Utils.Infrastructure.Core;

namespace Netfox.Web.BL.Queries
{
    public class CapturesQuery : AppQueryBase<CaptureDTO>
    {
        public Guid CaptureId { get; set; }

        public CapturesQuery(IUnitOfWorkProvider unitOfWorkProvider) : base(unitOfWorkProvider) { }

        public Guid InvestigationId { get; set; }

        protected override IQueryable<CaptureDTO> GetQueryable()
        {
            return this.Context.CapturesStats
            .Where(s => s.Investigation.Id == InvestigationId)
            .ProjectTo<CaptureDTO>();
        }
    }

}