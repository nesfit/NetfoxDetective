using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.BusinessPack.Controls;
using DotVVM.Framework.ViewModel;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Facades;

namespace Netfox.Web.App.ViewModels.Investigation
{
    public class ExportDetailViewModel : LayoutInvestigationViewModel
    {
        public override string Title => "Export Detail";
        public List<Guid> SelectedExportedObjectBase { get; set; } = new List<Guid>();
        public List<SnooperExportObjectListDTO> ExportedObject { get; set; }

        [Bind(Direction.ServerToClient)]
        public BusinessPackDataSet<KeyValuePair<string, string>> ExportedObjectDetail { get; set; } = new BusinessPackDataSet<KeyValuePair<string, string>>();

        public ExportDetailViewModel(InvestigationFacade investigationFacade, HangfireFacade hangfireFacade, CaptureFacade captureFacade, ExportFacade exportFacade) : base(investigationFacade, hangfireFacade, captureFacade, exportFacade) { }

        #region Overrides of LayoutInvestigationViewModel
        public override Task PreRender()
        {
            
            var investigationId = Guid.Parse(this.Context.Parameters["InvestigationId"].ToString());
            var exportId = Guid.Parse(this.Context.Parameters["ExportId"].ToString());
          
            return base.PreRender();
        }
        #endregion
    }

    
}
