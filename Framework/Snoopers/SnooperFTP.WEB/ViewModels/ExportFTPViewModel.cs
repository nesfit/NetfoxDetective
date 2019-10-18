using System;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using Netfox.Core.Enums;
using Netfox.Core.Interfaces;
using Netfox.SnooperFTP.Models;
using Netfox.SnooperFTP.WEB.DTO;
using Netfox.SnooperFTP.WEB.Facade;
using Netfox.Web.App.ViewModels.Investigation;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Facades;

namespace Netfox.SnooperFTP.WEB.ViewModels
{
    public class ExportFTPViewModel : ExportBaseViewModel
    {

        public GridViewDataSet<SnooperFTPListDTO> ExportObjects { get; set; } = new GridViewDataSet<SnooperFTPListDTO>();

        public ExportFilterDTO Filters { get; set; } = new ExportFilterDTO();

        private ExportFTPFacade ExportFTPFacade { get; set; }

        public ExportFTPViewModel(InvestigationFacade investigationFacade, HangfireFacade hangfireFacade, CaptureFacade captureFacade, ExportFacade exportFacade, SnooperFTPWeb snooperInfo, ExportFTPFacade exportFTPFacade) : base(investigationFacade, hangfireFacade, captureFacade, exportFacade)
        {
            this.SnooperInfo = snooperInfo;
            this.ExportFTPFacade = exportFTPFacade;
        }

        public override Task PreRender()
        {
            if(!this.Context.IsPostBack)
            {
                this.ExportObjects.PagingOptions.PageSize = 15;
                this.ExportObjects.SortingOptions.SortDescending = false;
                this.ExportObjects.SortingOptions.SortExpression = nameof(SnooperFTPListDTO.FirstSeen);
                this.ExportFTPFacade.InitFilter(this.Filters, this.InvestigationId);
                this.Clear();

            }

            if(this.ExportObjects.IsRefreshRequired)
            {
                this.ExportFTPFacade.FillDataSet(this.ExportObjects, this.InvestigationId, this.Filters);

                foreach (var item in ExportObjects.Items)
                {
                    if (item.Command == "DATA") { item.Value = "/" + item.Value.Replace(this.AppPath, String.Empty).Replace("\\", "/"); }
                }
            }

            return base.PreRender();
        }

        public void Filter() { }

        public void Clear()
        {
            this.Filters.SearchText = String.Empty;
            this.Filters.DurationTo = this.Filters.DurationMax.ToString("dd.MM.yyyy HH:mm:ss");
            this.Filters.DurationFrom = this.Filters.DurationMin.ToString("dd.MM.yyyy HH:mm:ss");
        }

        public void UpdateInvestigationMenu()
        {
            this.ExportObjects.IsRefreshRequired = false;
            base.UpdateInvestigationMenu();
        }
    }
}
