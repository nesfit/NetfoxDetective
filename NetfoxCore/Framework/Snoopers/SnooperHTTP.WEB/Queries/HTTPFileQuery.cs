using System;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using AutoMapper.QueryableExtensions;
using DotVVM.Framework.Controls;
using Netfox.Snoopers.SnooperHTTP.Models;
using Netfox.Snoopers.SnooperHTTP.WEB.DTO;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Providers;
using Netfox.Web.BL.Queries;

namespace Netfox.Snoopers.SnooperHTTP.WEB.Queries
{
    public class HTTPFileQuery : NetfoxQueryBase<SnooperHTTPFileDTO>
    {
        public ExportFilterDTO Filters { get; set; }

        public string SortExpression { get; set; }

        public bool SortDescending { get; set; }

        public bool FilterImage { get; set; } = false;

        public HTTPFileQuery(NetfoxUnitOfWorkProvider unitOfWorkProvider) : base(unitOfWorkProvider) {}

        private IQueryable<SnooperExportedDataObjectHTTP> GetBaseQuery()
        {
            var query = (IQueryable<SnooperExportedDataObjectHTTP>)this.unitOfWorkProvider.TryGetContext().Set<SnooperExportedDataObjectHTTP>()
                .Where(m => m.Message.HTTPContent.Content != null && m.Message.HttpResponseHeader.StatusCode == "200");
            if(this.FilterImage)
            {
                query = query.Where(m => m.Message.HttpResponseHeader.Fields.SerializedValue.Contains("\"Content-Type\":[\"image/"));
            }

            /* Apply Filters */
            query = this.ApplyFilters(query);

            /* Ordering */
            query = this.AddSorting(query);

            return query;
        }

        protected override IQueryable<SnooperHTTPFileDTO> GetQueryable()
        {
            // Base Query
            var query = this.GetBaseQuery();
        
            return query.ProjectTo<SnooperHTTPFileDTO>();
         }

        public void FillDataSet(GridViewDataSet<SnooperHTTPFileDTO> dataSet)
        {
            dataSet.PagingOptions.TotalItemsCount = this.GetBaseQuery().Count();
            dataSet.Items = this.GetBaseQuery().Skip(dataSet.PagingOptions.PageIndex * dataSet.PagingOptions.PageSize)
                .Take(dataSet.PagingOptions.PageSize).ToList().AsQueryable().ProjectTo<SnooperHTTPFileDTO>().ToList();
        }

        private IQueryable<SnooperExportedDataObjectHTTP> ApplyFilters(IQueryable<SnooperExportedDataObjectHTTP> baseQuery)
         {
            var query = baseQuery;

            if (this.Filters != null)
            {

                if (!String.IsNullOrEmpty(this.Filters.DurationFrom))
                {
                    var durFrom = DateTime.ParseExact(this.Filters.DurationFrom, "dd.MM.yyyy HH:mm:ss", new DateTimeFormatInfo());
                    if (DateTime.Compare(durFrom, this.Filters.DurationMin) != 0) { query = query.Where(c => c.Message.TimeStamp >= durFrom); }
                }
                if (!String.IsNullOrEmpty(this.Filters.DurationTo))
                {
                    var durTo = DateTime.ParseExact(this.Filters.DurationTo, "dd.MM.yyyy HH:mm:ss", new DateTimeFormatInfo());
                    if (DateTime.Compare(durTo, this.Filters.DurationMax) != 0) { query = query.Where(c => c.Message.TimeStamp <= durTo); }
                }

                if (!String.IsNullOrEmpty(this.Filters.SearchText))
                {
                    var expressions = this.Filters.SearchText.Split(' ');
                    foreach (var exp in expressions) { query = query.Where(e => e.SourceEndpointString.Contains(exp) || e.DestinationEndpointString.Contains(exp) || e.Message.HttpResponseHeader.Fields.SerializedValue.Contains(exp)); }
                }
            }

            return query;
         }

        private IQueryable<SnooperExportedDataObjectHTTP> AddSorting(IQueryable<SnooperExportedDataObjectHTTP> baseQuery)
        {
            var query = baseQuery;

            query = query.OrderBy<SnooperExportedDataObjectHTTP>(this.SortExpression + (this.SortDescending ? " descending" : ""));

            return query;
        }
   
        public void InitFilter(ExportFilterDTO filter)
        {
            // Base Query
            var query = (IQueryable<SnooperExportedDataObjectHTTP>)this.unitOfWorkProvider.TryGetContext().Set<SnooperExportedDataObjectHTTP>()
                .Where(m => m.Message.HTTPContent.Content != null);
            if (this.FilterImage)
            {
                query = query.Where(m => m.Message.HttpResponseHeader.Fields.SerializedValue.Contains("\"Content-Type\":[\"image/"));
            }

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
 