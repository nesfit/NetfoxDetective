using System;
using AutoMapper;
using DotVVM.Framework.Controls;
using Netfox.SnooperDNS.Models;
using Netfox.SnooperDNS.WEB.DTO;
using Netfox.SnooperDNS.WEB.Queries;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Facades.Base;
using Netfox.Web.BL.Providers;

namespace Netfox.SnooperDNS.WEB.Facade
{
    public class ExportDNSFacade : NetfoxFacadeBase
    {
        public Func<DNSQuery> MessageFactory { get; set; }

        public ExportDNSFacade(NetfoxUnitOfWorkProvider unitOfWorkProvider, NetfoxRepositoryProvider repositoryProvider) : base(unitOfWorkProvider, repositoryProvider) { }

        public void FillMessages(Guid investigationId, GridViewDataSet<SnooperDNSListDTO> dataset, ExportFilterDTO filter)
        {
            using (var uow = this.UnitOfWorkProvider.Create(investigationId))
            {
                var q = this.MessageFactory();
                q.Filters = filter;
                q.SortExpression = dataset.SortingOptions.SortExpression;
                q.SortDescending = dataset.SortingOptions.SortDescending;

                q.FillDataSet(dataset);
            }
        }

        public SnooperDNSDetailDTO GetDetail(Guid investigationId, Guid objectId)
        {
            
            using (var uow = this.UnitOfWorkProvider.Create(investigationId))
            {
                var repository = this.RepositoryProvider.Create<SnooperExportedDataObjectDNS>(investigationId, uow);
                return Mapper.Map<SnooperDNSDetailDTO>(repository.GetById(objectId));
            }
            
        }

        public void InitFilter(ExportFilterDTO filter, Guid investigationId)
        {
            using (var uow = this.UnitOfWorkProvider.Create(investigationId))
            {
                var q = this.MessageFactory();
                q.InitFilter(filter);
            }
        }
    }
}
