using System;
using DotVVM.Framework.Controls;
using Netfox.SnooperFTP.WEB.DTO;
using Netfox.SnooperFTP.WEB.Queries;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Facades.Base;
using Netfox.Web.BL.Providers;

namespace Netfox.SnooperFTP.WEB.Facade
{
    public class ExportFTPFacade : NetfoxFacadeBase
    {
        public Func<FTPExportObjectQuery> ExportObjectFactory { get; set; }

        public ExportFTPFacade(NetfoxUnitOfWorkProvider unitOfWorkProvider, NetfoxRepositoryProvider repositoryProvider) : base(unitOfWorkProvider, repositoryProvider) { }

        public void InitFilter(ExportFilterDTO filter, Guid investigationId)
        {
            using (var uow = this.UnitOfWorkProvider.Create(investigationId))
            {
                var q = this.ExportObjectFactory();
                q.InitFilter(filter);

            }
        }

        public void FillDataSet(GridViewDataSet<SnooperFTPListDTO> dataset, Guid investigationId, ExportFilterDTO filter)
        {
            using(var uow = this.UnitOfWorkProvider.Create(investigationId))
            {
                var q = ExportObjectFactory();
                q.Filters = filter;
                q.SortExpression = dataset.SortingOptions.SortExpression;
                q.SortDescending = dataset.SortingOptions.SortDescending;

                q.FillDataSet(dataset);
            }
        }
    }
}
