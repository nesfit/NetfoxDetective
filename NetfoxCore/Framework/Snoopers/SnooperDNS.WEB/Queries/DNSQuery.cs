using System;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using AutoMapper.QueryableExtensions;
using DotVVM.Framework.Controls;
using Netfox.Snoopers.SnooperDNS.Models;
using Netfox.Snoopers.SnooperDNS.WEB.DTO;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Providers;
using Netfox.Web.BL.Queries;

namespace Netfox.Snoopers.SnooperDNS.WEB.Queries
{
    public class DNSQuery : NetfoxQueryBase<SnooperDNSListDTO>
    {
        public ExportFilterDTO Filters { get; set; }

        public string SortExpression { get; set; }

        public bool SortDescending { get; set; }

        public DNSQuery(NetfoxUnitOfWorkProvider unitOfWorkProvider) : base(unitOfWorkProvider) {}

        private IQueryable<SnooperExportedDataObjectDNS> GetBaseQuery()
        {
            var query = (IQueryable<SnooperExportedDataObjectDNS>)this.unitOfWorkProvider.TryGetContext().Set<SnooperExportedDataObjectDNS>();

            /* Apply Filters */
            query = this.ApplyFilters(query);

            /* Ordering */
            query = this.AddSorting(query);

            return query;
        }

        protected override IQueryable<SnooperDNSListDTO> GetQueryable()
        {
            // Base Query
            var query = this.GetBaseQuery();
        
            return query.ProjectTo<SnooperDNSListDTO>();
         }

        public void FillDataSet(GridViewDataSet<SnooperDNSListDTO> dataSet)
        {
            dataSet.PagingOptions.TotalItemsCount = this.GetBaseQuery().Count();
            dataSet.Items = this.GetBaseQuery().Skip(dataSet.PagingOptions.PageIndex * dataSet.PagingOptions.PageSize)
                .Take(dataSet.PagingOptions.PageSize).ToList().AsQueryable().ProjectTo<SnooperDNSListDTO>().ToList();
        }

        private IQueryable<SnooperExportedDataObjectDNS> ApplyFilters(IQueryable<SnooperExportedDataObjectDNS> baseQuery)
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
                    foreach(var exp in expressions)
                    {
                        query = query.Where(e => e.SourceEndpointString.Contains(exp) ||
                             e.DestinationEndpointString.Contains(exp) || 
                             e.__MessageId.ToString().Contains(exp) ||
                             e.__Flags.ToString().Contains(exp) 
                             );
                    }
                }
            }

            return query;
         }

        private IQueryable<SnooperExportedDataObjectDNS> AddSorting(IQueryable<SnooperExportedDataObjectDNS> baseQuery)
        {
            var query = baseQuery;

            query = query.OrderBy<SnooperExportedDataObjectDNS>(this.SortExpression + (this.SortDescending ? " descending" : ""));

            return query;
        }
   
        public void InitFilter(ExportFilterDTO filter)
        {
            // Base Query
            var query = (IQueryable<SnooperExportedDataObjectDNS>) this.unitOfWorkProvider.TryGetContext().Set<SnooperExportedDataObjectDNS>();

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
 