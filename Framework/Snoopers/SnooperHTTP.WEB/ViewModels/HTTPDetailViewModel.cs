using System;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using Netfox.Core.Interfaces;
using Netfox.SnooperHTTP.WEB.DTO;
using Netfox.SnooperHTTP.WEB.Facade;
using Netfox.Web.App.ViewModels;

namespace Netfox.SnooperHTTP.WEB.ViewModels
{
    public class HTTPDetailViewModel : BlankLayoutViewModel
    {
        [FromQuery("messageid")]
        public Guid MessageId { get; set; } = Guid.Empty;

        [FromRoute("InvestigationId")]
        public Guid InvestigationId { get; set; }

        public SnooperHTTPDetailDTO Message { get; set; }

        private ExportHTTPFacade Facade { get; set; }

        public HTTPDetailViewModel(ExportHTTPFacade facade) { this.Facade = facade; }

        #region Overrides of DotvvmViewModelBase
        public override Task PreRender()
        {
            Message = this.Facade.GetMessage(InvestigationId, MessageId);
            return base.PreRender();
        }
        #endregion
    }
}
