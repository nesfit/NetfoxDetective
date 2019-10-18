using System;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using Netfox.SnooperDNS.WEB.DTO;
using Netfox.SnooperDNS.WEB.Facade;
using Netfox.Web.App.ViewModels;

namespace Netfox.SnooperDNS.WEB.ViewModels
{
    public class DNSDetailViewModel : BlankLayoutViewModel
    {
        [FromQuery("objectid")]
        public Guid ObjectId { get; set; } = Guid.Empty;

        [FromRoute("InvestigationId")]
        public Guid InvestigationId { get; set; }

        public SnooperDNSDetailDTO ExportedObject { get; set; }

        private ExportDNSFacade Facade { get; set; }

        public DNSDetailViewModel(ExportDNSFacade facade) { this.Facade = facade; }

        #region Overrides of DotvvmViewModelBase
        public override Task PreRender()
        {
            this.ExportedObject = this.Facade.GetDetail(this.InvestigationId, this.ObjectId);
            return base.PreRender();
        }
        #endregion
    }
}
