using System;
using System.Globalization;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DotVVM.Framework.Controls;
using Netfox.Core.Interfaces.Model.Exports;
using Netfox.Framework.Models.Snoopers;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Providers;

namespace Netfox.Web.BL.Queries
{
    public class ChatMessageQuery : NetfoxQueryBase<ExportChatMessageDTO>
    {
        public ExportFilterDTO Filters { get; set; }

        public string SortExpression { get; set; }

        public bool SortDescending { get; set; }

        public ChatMessageQuery(NetfoxUnitOfWorkProvider unitOfWorkProvider) : base(unitOfWorkProvider) {}

        private IQueryable<IChatMessage> GetBaseQuery()
        {
            var query = this.unitOfWorkProvider.TryGetContext().Set<SnooperExportedObjectBase>().ToList().OfType<IChatMessage>().ToList().AsQueryable();

            /* Apply Filters */
            query = ApplyFilters(query);

            /* Ordering */
            query = AddSorting(query);

            return query;
        }

        protected override IQueryable<ExportChatMessageDTO> GetQueryable()
        {
            // Base Query
            var query = this.GetBaseQuery();
        
            return query.ProjectTo<ExportChatMessageDTO>();
         }

        public void FillDataSet(GridViewDataSet<ExportChatMessageDTO> dataSet)
        {
            dataSet.PagingOptions.TotalItemsCount = this.GetBaseQuery().Count();
            dataSet.Items = this.GetBaseQuery().OrderBy(i => i.FirstSeen)
                .Skip(dataSet.PagingOptions.PageIndex * dataSet.PagingOptions.PageSize)
                .Take(dataSet.PagingOptions.PageSize).ToList().AsQueryable().ProjectTo<ExportChatMessageDTO>().ToList();
        }

        private IQueryable<IChatMessage> ApplyFilters(IQueryable<IChatMessage> baseQuery)
         {
            var query = baseQuery;

            if (this.Filters != null)
            {
                if (!String.IsNullOrEmpty(this.Filters.DurationFrom))
                {
                    var durFrom = DateTime.ParseExact(this.Filters.DurationFrom, "dd.MM.yyyy HH:mm:ss", new DateTimeFormatInfo());
                    if (DateTime.Compare(durFrom, this.Filters.DurationMin) != 0) { query = query.Where(c => c.FirstSeen >= durFrom); }
                }
                if (!String.IsNullOrEmpty(this.Filters.DurationTo))
                {
                    var durTo = DateTime.ParseExact(this.Filters.DurationTo, "dd.MM.yyyy HH:mm:ss", new DateTimeFormatInfo());
                    if (DateTime.Compare(durTo, this.Filters.DurationMax) != 0) { query = query.Where(c => c.FirstSeen <= durTo); }
                }

                if (!String.IsNullOrEmpty(this.Filters.SearchText))
                {
                    var expressions = this.Filters.SearchText.Split(' ');
                    foreach (var exp in expressions) { query = query.Where(e => e.Receiver.Contains(exp) || e.Sender.Contains(exp) || e.Message.Contains(exp)); }
                }
            }

            return query;
         }

        private IQueryable<IChatMessage> AddSorting(IQueryable<IChatMessage> baseQuery)
        {
            var query = baseQuery;

            // TODO: 20210425 - repair sorting
            // switch (this.SortExpression)
            // {
            //     case "SourceEndPoint":
            //         query = query.OrderBy("SourceEndPointIPAddressData" + (this.SortDescending ? " descending" : "") + ", SourceEndPointPort" + (this.SortDescending ? " descending" : ""));
            //         break;
            //     case "DestinationEndPoint":
            //         query = query.OrderBy("DestinationEndPointIPAddressData" + (this.SortDescending ? " descending" : "") + ", DestinationEndPointPort" + (this.SortDescending ? " descending" : ""));
            //         break;
            //     default:
            //         query = query.OrderBy(this.SortExpression + (this.SortDescending ? " descending" : ""));
            //         break;
            // }

            return query;
        }
   
       
        public void InitChatMessageFilter(ExportFilterDTO filter)
        {
            var query = this.unitOfWorkProvider.TryGetContext().Set<SnooperExportedObjectBase>()
                .ToList().OfType<IChatMessage>().ToList().AsQueryable();

            var durationMax = query.Select(e => e.FirstSeen).DefaultIfEmpty().Max();
            var durationMin = query.Select(e => e.FirstSeen).DefaultIfEmpty().Min();

            filter.DurationMin = new DateTime(
                durationMin.Ticks - (durationMin.Ticks % TimeSpan.TicksPerSecond),
                durationMin.Kind
            );
            filter.DurationMax = new DateTime(
                durationMax.Ticks - (durationMax.Ticks % TimeSpan.TicksPerSecond),
                durationMax.Kind
            );
        }
    }
}
 