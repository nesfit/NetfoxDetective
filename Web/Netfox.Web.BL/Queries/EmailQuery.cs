using System;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DotVVM.Framework.Controls;
using Netfox.Core.Interfaces.Model.Exports;
using Netfox.Framework.Models.Snoopers;
using Netfox.Framework.Models.Snoopers.Email;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Providers;

namespace Netfox.Web.BL.Queries
{
    public class EmailQuery : NetfoxQueryBase<ExportEmailDTO>
    {
        public ExportFilterDTO Filters { get; set; }

        public string SortExpression { get; set; }

        public bool SortDescending { get; set; }

        public EmailQuery(NetfoxUnitOfWorkProvider unitOfWorkProvider) : base(unitOfWorkProvider) {}

        private IQueryable<MIMEpart> GetBaseQuery()
        {
            var query = this.unitOfWorkProvider.TryGetContext().Set<MIMEpart>().Where(e => e.From != null);

            var a = query.ToList();
            /* Apply Filters */
            query = ApplyFilters(query);

            /* Ordering */
            query = AddSorting(query);

            return query;
        }

        protected override IQueryable<ExportEmailDTO> GetQueryable()
        {
            // Base Query
            var query = this.GetBaseQuery();
        
            return query.ProjectTo<ExportEmailDTO>();
         }

        public void FillDataSet(GridViewDataSet<ExportEmailDTO> dataSet)
        {
            dataSet.PagingOptions.TotalItemsCount = this.GetBaseQuery().Count();
            dataSet.Items = this.GetBaseQuery().Skip(dataSet.PagingOptions.PageIndex * dataSet.PagingOptions.PageSize)
                .Take(dataSet.PagingOptions.PageSize).ToList().AsQueryable().ProjectTo<ExportEmailDTO>().ToList();
        }

        private IQueryable<MIMEpart> ApplyFilters(IQueryable<MIMEpart> baseQuery)
         {
            var query = baseQuery;

            if (this.Filters != null)
            {

                if (!String.IsNullOrEmpty(this.Filters.SearchText))
                {
                    var expressions = this.Filters.SearchText.Split(' ');
                    foreach (var exp in expressions) { query = query.Where(e => e.From.Contains(exp) || e.To.Contains(exp) || e.Subject.Contains(exp) || e.Cc.Contains(exp) || e.Bcc.Contains(exp)); }
                }
            }

            return query;
         }

        private IQueryable<MIMEpart> AddSorting(IQueryable<MIMEpart> baseQuery)
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

        public ExportEmailDTO GetEmail(Guid emailId)
        {
            var email = this.unitOfWorkProvider.TryGetContext().Set<MIMEpart>().Single(e => e.Id == emailId);
            return Mapper.Map<ExportEmailDTO>(email);
        }

        public void FillAttachmentsOfEmail(GridViewDataSet<EmailAttachmentDTO> dataSet, Guid emailId)
        {
            var q = "SELECT * FROM MIMEparts WHERE MIMEpart_Id = '" + emailId + "'";
            var attachments = this.unitOfWorkProvider.TryGetContext().Set<MIMEpart>().SqlQuery(q).ToList();
            dataSet.PagingOptions.TotalItemsCount = attachments.Count();
            dataSet.Items = attachments.Skip(dataSet.PagingOptions.PageIndex * dataSet.PagingOptions.PageSize)
                .Take(dataSet.PagingOptions.PageSize).ToList().AsQueryable().ProjectTo<EmailAttachmentDTO>().ToList();
        }

        public void GetAddressOfEmail(ExportEmailDTO export, Guid emailId)
        {
            var q = "SELECT * FROM MIMEemail e LEFT JOIN SnooperExportedObjectBases s On e.Id = s.Id LEFT JOIN RawEMail r On e.Id = r.Id WHERE DocumentRoot_Id = '" + emailId + "'";
            var email = this.unitOfWorkProvider.TryGetContext().Set<MIMEemail>().SqlQuery(q).ToList().Single();
            export.Source = email.SourceEndpointString;
            export.Destination = email.DestinationEndpointString;
        }


        public void InitEmailFilter(ExportFilterDTO filter)
        {
           
        }
    }
}
 