using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using AutoMapper.QueryableExtensions;
using DotVVM.Framework.Controls;
using Netfox.Core.Interfaces.Model.Exports;
using Netfox.Framework.Models.Snoopers;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Providers;

namespace Netfox.Web.BL.Queries
{
    public class CallStreamQuery : NetfoxQueryBase<ExportCallStreamDTO>
    {
        public ExportFilterDTO Filters { get; set; }

        public string SortExpression { get; set; }

        public bool SortDescending { get; set; }

        public CallStreamQuery(NetfoxUnitOfWorkProvider unitOfWorkProvider) : base(unitOfWorkProvider) {}

        private IQueryable<ICallStream> GetBaseQuery()
        {
            var query = this.unitOfWorkProvider.TryGetContext().Set<SnooperExportedObjectBase>().ToList().OfType<ICallStream>().ToList().AsQueryable();
            /* Apply Filters */
            query = ApplyFilters(query);

            /* Ordering */
            query = AddSorting(query);

            return query;
        }

        protected override IQueryable<ExportCallStreamDTO> GetQueryable()
        {
            // Base Query
            var query = this.GetBaseQuery();
        
            return query.ProjectTo<ExportCallStreamDTO>();
         }

        public void FillDataSet(GridViewDataSet<ExportCallStreamDTO> dataSet)
        {
            dataSet.PagingOptions.TotalItemsCount = this.GetBaseQuery().Count();
            dataSet.Items = this.GetBaseQuery().Skip(dataSet.PagingOptions.PageIndex * dataSet.PagingOptions.PageSize)
                .Take(dataSet.PagingOptions.PageSize).ToList().AsQueryable().ProjectTo<ExportCallStreamDTO>().ToList();
        }

        private IQueryable<ICallStream> ApplyFilters(IQueryable<ICallStream> baseQuery)
         {
            var query = baseQuery;

            if (this.Filters != null)
            {
                if (!String.IsNullOrEmpty(this.Filters.DurationFrom))
                {
                    var durFrom = DateTime.ParseExact(this.Filters.DurationFrom, "dd.MM.yyyy HH:mm:ss", new DateTimeFormatInfo());
                    if (DateTime.Compare(durFrom, this.Filters.DurationMin) != 0) { query = query.Where(c => c.Start >= durFrom); }
                }
                if (!String.IsNullOrEmpty(this.Filters.DurationTo))
                {
                    var durTo = DateTime.ParseExact(this.Filters.DurationTo, "dd.MM.yyyy HH:mm:ss", new DateTimeFormatInfo());
                    if (DateTime.Compare(durTo, this.Filters.DurationMax) != 0) { query = query.Where(c => c.End <= durTo); }
                }

                if (!String.IsNullOrEmpty(this.Filters.SearchText))
                {
                    var expressions = this.Filters.SearchText.Split(' ');
                    foreach (var exp in expressions) { query = query.Where(e => e.From.Contains(exp) || e.To.Contains(exp)); }
                }
            }

            return query;
         }

        private IQueryable<ICallStream> AddSorting(IQueryable<ICallStream> baseQuery)
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
                default:
                    query = query.OrderBy(this.SortExpression + (this.SortDescending ? " descending" : ""));
                    break;
            }

            return query;
        }
   
       
        public void InitCallStreamFilter(ExportFilterDTO filter)
        {
            var query = this.unitOfWorkProvider.TryGetContext().Set<SnooperExportedObjectBase>().ToList().OfType<ICallStream>().ToList().AsQueryable();

            var durationMax = query.Select(e => e.End).DefaultIfEmpty().Max() ?? DateTime.MaxValue;
            var durationMin = query.Select(e => e.Start).DefaultIfEmpty().Min() ?? DateTime.MinValue;

           
            filter.DurationMin = new DateTime(
                durationMin.Ticks - (durationMin.Ticks % TimeSpan.TicksPerSecond),
                durationMin.Kind
            );
           
            filter.DurationMax = new DateTime(
                durationMax.Ticks - (durationMax.Ticks % TimeSpan.TicksPerSecond),
                durationMax.Kind
            );
        }

        public void FillCallStreamOfCall(GridViewDataSet<ExportCallStreamDTO> dataSet, IEnumerable<string> rtpAddress)
        {
            // Base Query
            var query = this.unitOfWorkProvider.TryGetContext().Set<SnooperExportedObjectBase>().Where(e => rtpAddress.Contains(e.SourceEndpointString) && rtpAddress.Contains(e.DestinationEndpointString)).ToList().OfType<ICallStream>().ToList().AsQueryable();

            dataSet.PagingOptions.TotalItemsCount = query.Count();
            dataSet.Items = query.Skip(dataSet.PagingOptions.PageIndex * dataSet.PagingOptions.PageSize)
                .Take(dataSet.PagingOptions.PageSize).ToList().AsQueryable().ProjectTo<ExportCallStreamDTO>().ToList();
        }
    }
}
 