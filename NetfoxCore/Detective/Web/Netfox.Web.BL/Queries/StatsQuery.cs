using System;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Netfox.Web.BL.DTO;
using Riganti.Utils.Infrastructure.Core;

namespace Netfox.Web.BL.Queries
{
    public class StatsQuery : AppQueryBase<ConvesationStatisticsDTO>
    {
        public Guid CaptureId { get; set; }

        public Guid InvestigationId { get; set; }

        public StatsQuery(IUnitOfWorkProvider unitOfWorkProvider) : base(unitOfWorkProvider) { }

        protected override IQueryable<ConvesationStatisticsDTO> GetQueryable()
        {
            if(CaptureId != Guid.Empty)
            {
                return this.Context.CapturesStats
                .Where(s => s.Id == this.CaptureId && s.Investigation.Id == this.InvestigationId)
                .ProjectTo<ConvesationStatisticsDTO>();
            }
            
           return this.Context.CapturesStats
                .Where(s => s.Investigation.Id == this.InvestigationId)
                .ProjectTo<ConvesationStatisticsDTO>();
            

        }

        public void GetInvestigationStats(InvestigationStatisticsDTO stats)
        {
            stats.CountCaptures = this.GetQueryable().Count();
            if(stats.CountCaptures > 0)
            {
                stats.TotalSize = this.GetQueryable().Sum(s => s.Size);
                stats.TotalL3Conversations = this.GetQueryable().Sum(s => s.TotalL3Conversations);
                stats.TotalL4Conversations = this.GetQueryable().Sum(s => s.TotalL4Conversations);
                stats.TotalL7Conversations = this.GetQueryable().Sum(s => s.TotalL7Conversations);
                stats.TotalFrames = this.GetQueryable().Sum(s => s.TotalFrames);
            }
        }
    }

}