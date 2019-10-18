using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Netfox.Web.BL.DTO;
using Netfox.Web.DAL.Entities;
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