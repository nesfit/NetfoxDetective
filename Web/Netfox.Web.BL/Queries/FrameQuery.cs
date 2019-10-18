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
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Facades;
using Netfox.Web.BL.Providers;
using Netfox.Web.DAL.Entities;
using Riganti.Utils.Infrastructure.Core;

namespace Netfox.Web.BL.Queries
{
    public class FrameQuery : NetfoxQueryBase<PmFrameBaseDTO>
    {
        public FrameFilterDTO Filters { get; set; }

        public Guid FilterId { get; set; }

        public string SortExpression { get; set; }

        public bool SortDescending { get; set; }

        public ConversationType FilterType { get; set; } = ConversationType.None;

        public FrameQuery(NetfoxUnitOfWorkProvider unitOfWorkProvider) : base(unitOfWorkProvider) {}

        private IQueryable<PmFrameBase> GetBaseQuery()
        {
            var query = (IQueryable<PmFrameBase>)this.unitOfWorkProvider.TryGetContext().Set<PmFrameBase>();

            switch (this.FilterType)
            {
                case ConversationType.L3:
                    query = query.Where(c => c.L3ConversationRefId == this.FilterId);
                    break;
                case ConversationType.L4:
                    query = query.Where(c => c.L4ConversationRefId == this.FilterId);
                    break;
                case ConversationType.L7:
                    query = query.Where(c => c.L7ConversationRefId == this.FilterId);
                    break;
                default:
                    query = query.Where(c => c.PmCaptureRefId == this.FilterId);
                    break;
            }


            /* Apply Filters */
            query = ApplyFilters(query);

            /* Ordering */
            query = AddSorting(query);

            return query;
        }

        protected override IQueryable<PmFrameBaseDTO> GetQueryable()
        {
            // Base Query
            var query = this.GetBaseQuery();
        
            return query.ProjectTo<PmFrameBaseDTO>();
         }

        public void FillDataSet(GridViewDataSet<PmFrameBaseDTO> dataSet)
        {
            dataSet.PagingOptions.TotalItemsCount = this.GetBaseQuery().Count();
            dataSet.Items = this.GetBaseQuery().Skip(dataSet.PagingOptions.PageIndex * dataSet.PagingOptions.PageSize)
                .Take(dataSet.PagingOptions.PageSize).ToList().AsQueryable().ProjectTo<PmFrameBaseDTO>().ToList();
        }

        private IQueryable<PmFrameBase> ApplyFilters(IQueryable<PmFrameBase> baseQuery)
         {
            var query = baseQuery;

            if (this.Filters != null)
            {
                if (this.Filters.BytesTo != this.Filters.BytesMax && this.Filters.BytesTo != 0) { query = query.Where(c => c.OriginalLength <= this.Filters.BytesTo); }

                if (this.Filters.BytesFrom != this.Filters.BytesMin && this.Filters.BytesFrom != 0) { query = query.Where(c => c.OriginalLength >= this.Filters.BytesFrom); }

                if (!String.IsNullOrEmpty(this.Filters.DurationFrom))
                {
                    var durFrom = DateTime.ParseExact(this.Filters.DurationFrom, "dd.MM.yyyy HH:mm:ss", new DateTimeFormatInfo());
                    if (DateTime.Compare(durFrom, this.Filters.DurationMin) != 0) { query = query.Where(c => c.TimeStamp >= durFrom); }
                }

                if (!String.IsNullOrEmpty(this.Filters.DurationTo))
                {
                    var durTo = DateTime.ParseExact(this.Filters.DurationTo, "dd.MM.yyyy HH:mm:ss", new DateTimeFormatInfo());
                    if (DateTime.Compare(durTo, this.Filters.DurationMax) != 0) { query = query.Where(c => c.TimeStamp <= durTo); }
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
                                query = query.Where(c => c.SrcAddressData == addressBytes || c.DstAddressData == addressBytes);
                            }
                        }
                        else // port
                        {
                            var success = Int64.TryParse(this.Filters.SearchText, out var port);
                            if (success)
                            {
                                query = query.Where(c => c.SrcPort == port || c.DstPort == port);
                            }
                         
                        }

                    }
                    else if (count == 1) //IPEndpoint
                    {
                        var splitEndpoint = this.Filters.SearchText.Split(':');
                        var success = IPAddress.TryParse(splitEndpoint[0], out var ipaddr);
                        var successPort = Int64.TryParse(splitEndpoint[1], out var port);
                        if (success && successPort)
                        {
                            var searchTextBytes = ipaddr.GetAddressBytes();
                            query = query.Where(c => (c.SrcAddressData == searchTextBytes && c.SrcPort == port) || (c.DstAddressData == searchTextBytes && c.DstPort == port));
                        }
                    }
                    else //IPv6Endpoint
                    {
                        var searchEndpoint = Regex.Replace(this.Filters.SearchText, "\\[", String.Empty);
                        var splitEndpoint = Regex.Split(searchEndpoint, "\\]:");
                        if (splitEndpoint.Count() > 1)
                        {
                            var addressBytes = IPAddress.Parse(splitEndpoint[0]).GetAddressBytes();
                            var port = Int64.Parse(splitEndpoint[1]);
                            query = query.Where(c => (c.SrcAddressData == addressBytes && c.SrcPort == port) || (c.DstAddressData == addressBytes && c.DstPort == port));

                        }
                        else
                        {
                            var addressBytes = IPAddress.Parse(splitEndpoint[0]).GetAddressBytes();
                            query = query.Where(c => c.SrcAddressData == addressBytes || c.DstAddressData == addressBytes);
                        }
                    }
                }
            }

            return query;
         }

        private IQueryable<PmFrameBase> AddSorting(IQueryable<PmFrameBase> baseQuery)
        {
            var query = baseQuery;

            switch (this.SortExpression)
            {
                case "SourceEndPoint":
                    query = query.OrderBy("SrcAddressData" + (this.SortDescending ? " descending" : "") + ", SrcPort" + (this.SortDescending ? " descending" : ""));
                    break;
                case "DestinationEndPoint":
                    query = query.OrderBy("DstAddressData" + (this.SortDescending ? " descending" : "") + ", DstPort" + (this.SortDescending ? " descending" : ""));
                    break;
                case "OriginalLength":
                    query = query.OrderBy("OriginalLength" + (this.SortDescending ? " descending" : ""));
                    break;
                default:
                    query = query.OrderBy(this.SortExpression + (this.SortDescending ? " descending" : ""));
                    break;
            }

            return query;
        }
    }
}
 