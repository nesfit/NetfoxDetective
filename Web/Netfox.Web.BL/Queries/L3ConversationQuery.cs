using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DotVVM.Framework.Controls;
using Netfox.Core.Enums;
using Netfox.Framework.Models;
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Providers;
using Netfox.Web.DAL.Entities;
using Riganti.Utils.Infrastructure.Core;

namespace Netfox.Web.BL.Queries
{
    public class L3ConversationQuery : NetfoxQueryBase<L3ConversationDTO>
    {
        public ConversationFilterDTO Filters { get; set; }

        public Guid CaptureId { get; set; }

        public string SortExpression { get; set; }

        public bool SortDescending { get; set; }

        public L3ConversationQuery(NetfoxUnitOfWorkProvider unitOfWorkProvider) : base(unitOfWorkProvider) {}

        private IQueryable<L3Conversation> GetBaseQuery()
        {
            var query = this.unitOfWorkProvider.TryGetContext().Set<L3Conversation>().Where(c => c.Captures.Any(cc => cc.Id == this.CaptureId));

            /* Apply Filters */
            query = ApplyFilters(query);

            /* Ordering */
            query = AddSorting(query);

            return query;
        }

        protected override IQueryable<L3ConversationDTO> GetQueryable()
        {
            // Base Query
            var query = this.GetBaseQuery();
        
            return query.ProjectTo<L3ConversationDTO>();
         }

        public void FillDataSet(GridViewDataSet<L3ConversationDTO> dataSet)
        {
            dataSet.PagingOptions.TotalItemsCount = this.GetBaseQuery().Count();
            dataSet.Items = this.GetBaseQuery().Skip(dataSet.PagingOptions.PageIndex * dataSet.PagingOptions.PageSize)
                .Take(dataSet.PagingOptions.PageSize).ToList().AsQueryable().ProjectTo<L3ConversationDTO>().ToList();
        }

        private IQueryable<L3Conversation> ApplyFilters(IQueryable<L3Conversation> baseQuery)
         {
             var query = baseQuery;
             if (this.Filters.FramesTo != this.Filters.FrameMax && this.Filters.FramesTo != 0) { query = query.Where(c => c.Frames.Count <= this.Filters.FramesTo); }
             if (this.Filters.FramesFrom != this.Filters.FrameMin && this.Filters.FramesTo != 0) { query = query.Where(c => c.Frames.Count >= this.Filters.FramesFrom); }
             if (this.Filters.BytesTo != this.Filters.BytesMax && this.Filters.BytesTo != 0) { query = query.Where(c => c.ConversationFlowStatistics.Sum(cs => cs.Bytes) <= this.Filters.BytesTo); }
             if (this.Filters.BytesFrom != this.Filters.BytesMin && this.Filters.BytesFrom != 0) { query = query.Where(c => c.ConversationFlowStatistics.Sum(cs => cs.Bytes) >= this.Filters.BytesFrom); }

             if (!String.IsNullOrEmpty(this.Filters.DurationFrom))
             {
                 var durFrom = DateTime.ParseExact(this.Filters.DurationFrom, "dd.MM.yyyy HH:mm:ss", new DateTimeFormatInfo());
                 if (DateTime.Compare(durFrom, this.Filters.DurationMin) != 0) { query = query.Where(c => c.FirstSeen >= durFrom); }
             }
             if (!String.IsNullOrEmpty(this.Filters.DurationTo))
             {
                 var durTo = DateTime.ParseExact(this.Filters.DurationTo, "dd.MM.yyyy HH:mm:ss", new DateTimeFormatInfo());
                 if (DateTime.Compare(durTo, this.Filters.DurationMax) != 0) { query = query.Where(c => c.LastSeen <= durTo); }
             }

             if (!String.IsNullOrEmpty(this.Filters.SearchText))
             {
                 var success = IPAddress.TryParse(this.Filters.SearchText, out var ipaddr);
                 if (success)
                 {
                     var searchTextBytes = ipaddr.GetAddressBytes();
                     query = query.Where(c => c.IPAddress1Data == searchTextBytes || c.IPAddress2Data == searchTextBytes);
                 }

             }

             return query;
         }

        private IQueryable<L3Conversation> AddSorting(IQueryable<L3Conversation> baseQuery)
        {
            var query = baseQuery;

            switch (this.SortExpression)
            {
                case "IPAddress1":
                    query = query.OrderBy("IPAddress1Data" + (this.SortDescending ? " descending" : ""));
                    break;
                case "IPAddress2":
                    query = query.OrderBy("IPAddress2Data" + (this.SortDescending ? " descending" : ""));
                    break;
                case "UpFlowFramesCount":
                    query = query.OrderBy("ConversationFlowStatistics.Where(FlowDirection = @0).Sum(Frames)" + (this.SortDescending ? " descending" : ""), DaRFlowDirection.up);
                    break;
                case "DownFlowFramesCount":
                    query = query.OrderBy("ConversationFlowStatistics.Where(FlowDirection = @0).Sum(Frames)" + (this.SortDescending ? " descending" : ""), DaRFlowDirection.down);
                    break;
                case "UpFlowBytes":
                    query = query.OrderBy("ConversationFlowStatistics.Where(FlowDirection = @0).Sum(Bytes)" + (this.SortDescending ? " descending" : ""), DaRFlowDirection.up);
                    break;
                case "DownFlowBytes":
                    query = query.OrderBy("ConversationFlowStatistics.Where(FlowDirection = @0).Sum(Bytes)" + (this.SortDescending ? " descending" : ""), DaRFlowDirection.down);
                    break;
                case "MalformedFrames":
                    query = query.OrderBy("ConversationFlowStatistics.Sum(MalformedFrames)" + (this.SortDescending ? " descending" : ""));
                    break;
                default:
                    query = query.OrderBy(this.SortExpression + (this.SortDescending ? " descending" : ""));
                    break;
            }

            return query;
        }
    }
}
 