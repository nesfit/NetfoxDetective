using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DotVVM.Framework.Controls;
using Netfox.Core.Enums;
using Netfox.Framework.Models;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Facades;
using Netfox.Web.BL.Providers;
using Netfox.Web.DAL.Entities;
using Riganti.Utils.Infrastructure.Core;

namespace Netfox.Web.BL.Queries
{
    public class L7ConversationQuery : NetfoxQueryBase<L7ConversationDTO>
    {
        public ConversationFilterDTO Filters { get; set; }

        public Guid FilterId { get; set; }

        public string SortExpression { get; set; }

        public bool SortDescending { get; set; }

        public ConversationType FilterType { get; set; } = ConversationType.None;

        public L7ConversationQuery(NetfoxUnitOfWorkProvider unitOfWorkProvider) : base(unitOfWorkProvider) {}

        private IQueryable<L7Conversation> GetBaseQuery()
        {
            var query = (IQueryable<L7Conversation>)this.unitOfWorkProvider.TryGetContext().Set<L7Conversation>();

            switch (this.FilterType)
            {
                case ConversationType.L3:
                    query = query.Where(c => c.L3ConversationRefId == this.FilterId);
                    break;
                case ConversationType.L4:
                    query = query.Where(c => c.L4ConversationRefId == this.FilterId);
                    break;
                default:
                    query = query.Where(c => c.Captures.Any(cc => cc.Id == this.FilterId));
                    break;
            }

            /* Apply Filters */
            query = ApplyFilters(query);

            /* Ordering */
            query = AddSorting(query);

            return query;
        }

        protected override IQueryable<L7ConversationDTO> GetQueryable()
        {
            // Base Query
            var query = this.GetBaseQuery();
        
            return query.ProjectTo<L7ConversationDTO>();
         }

        public void FillDataSet(GridViewDataSet<L7ConversationDTO> dataSet)
        {
            dataSet.PagingOptions.TotalItemsCount = this.GetBaseQuery().Count();
            dataSet.Items = this.GetBaseQuery().Skip(dataSet.PagingOptions.PageIndex * dataSet.PagingOptions.PageSize)
                .Take(dataSet.PagingOptions.PageSize).ToList().AsQueryable().ProjectTo<L7ConversationDTO>().ToList();
        }

        private IQueryable<L7Conversation> ApplyFilters(IQueryable<L7Conversation> baseQuery)
         {
            var query = baseQuery;

            if (this.Filters != null)
            {
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
                    var count = this.Filters.SearchText.Count(f => f == ':');
                    if (count == 0)
                    {
                        // IP address
                        if (this.Filters.SearchText.Count(f => f == '.') > 0)
                        {
                            var success = IPAddress.TryParse(this.Filters.SearchText, out var ipaddr);
                            if (success)
                            {
                                var addressBytes = ipaddr.GetAddressBytes();
                                query = query.Where(c => c.L4Conversation.SourceEndPointIPAddressData == addressBytes || c.L4Conversation.DestinationEndPointIPAddressData == addressBytes);
                            }

                        }
                        else // port
                        {
                            var success = Int64.TryParse(this.Filters.SearchText, out var port);
                            if(success)
                            {
                                query = query.Where(c => c.L4Conversation.SourceEndPointPort == port || c.L4Conversation.DestinationEndPointPort == port);
                            }

                        }

                    }
                    else if (count == 1) //IPEndpoint
                    {
                        var splitEndpoint = this.Filters.SearchText.Split(':');
                        var searchTextBytes = IPAddress.Parse(splitEndpoint[0]).GetAddressBytes();
                        var port = Int64.Parse(splitEndpoint[1]);
                        query = query.Where(c => (c.L4Conversation.SourceEndPointIPAddressData == searchTextBytes && c.L4Conversation.SourceEndPointPort == port) || (c.L4Conversation.DestinationEndPointIPAddressData == searchTextBytes && c.L4Conversation.DestinationEndPointPort == port));
                    }
                    else //IPv6Endpoint
                    {
                        var searchEndpoint = Regex.Replace(this.Filters.SearchText, "\\[", String.Empty);
                        var splitEndpoint = Regex.Split(searchEndpoint, "\\]:");
                        if (splitEndpoint.Count() > 1)
                        {
                            var addressBytes = IPAddress.Parse(splitEndpoint[0]).GetAddressBytes();
                            var port = Int64.Parse(splitEndpoint[1]);
                            query = query.Where(c => (c.L4Conversation.SourceEndPointIPAddressData == addressBytes && c.L4Conversation.SourceEndPointPort == port) || (c.L4Conversation.DestinationEndPointIPAddressData == addressBytes && c.L4Conversation.DestinationEndPointPort == port));

                        }
                        else
                        {
                            var addressBytes = IPAddress.Parse(splitEndpoint[0]).GetAddressBytes();
                            query = query.Where(c => c.L4Conversation.SourceEndPointIPAddressData == addressBytes || c.L4Conversation.DestinationEndPointIPAddressData == addressBytes);
                        }
                    }
                }
            }

            return query;
         }

        private IQueryable<L7Conversation> AddSorting(IQueryable<L7Conversation> baseQuery)
        {
            var query = baseQuery;

            switch (this.SortExpression)
            {
                case "SourceEndPoint":
                    query = query.OrderBy("SourceEndPointIPAddressData" + (this.SortDescending ? " descending" : "") + ", SourceEndPointPort" + (this.SortDescending ? " descending" : ""));
                    break;
                case "DestinationEndPoint":
                    query = query.OrderBy("DestinationEndPointIPAddressData" + (this.SortDescending ? " descending" : "") + ", DestinationEndPointPort" + (this.SortDescending ? " descending" : ""));
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
 