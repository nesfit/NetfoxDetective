using System;
using System.IO;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Storage;
using Netfox.Core.Interfaces;
using Netfox.SnooperHTTP.WEB.DTO;
using Netfox.SnooperHTTP.WEB.Facade;
using Netfox.Web.App.ViewModels.Investigation;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Facades;

namespace Netfox.SnooperHTTP.WEB.ViewModels
{
    public class ExportHTTPViewModel : ExportBaseViewModel
    {
        public ExportFilterDTO MessageFilter { get; set; } = new ExportFilterDTO();

        public ExportFilterDTO FileFilter { get; set; } = new ExportFilterDTO();

        public ExportFilterDTO ImageFilter { get; set; } = new ExportFilterDTO();

        private ExportHTTPFacade ExportHTTPFacade { get; set; }

        public GridViewDataSet<SnooperHTTPListDTO> Messages { get; set; } = new GridViewDataSet<SnooperHTTPListDTO>();

        public GridViewDataSet<SnooperHTTPFileDTO> Files { get; set; } = new GridViewDataSet<SnooperHTTPFileDTO>();

        public GridViewDataSet<SnooperHTTPFileDTO> Images { get; set; } = new GridViewDataSet<SnooperHTTPFileDTO>();

        public ExportHTTPViewModel(
            InvestigationFacade investigationFacade,
            HangfireFacade hangfireFacade,
            CaptureFacade captureFacade,
            ExportFacade exportFacade,
            SnooperHTTPWeb snooperInfo,
            ExportHTTPFacade exportFTPFacade) : base(investigationFacade, hangfireFacade, captureFacade, exportFacade)
        {
            this.SnooperInfo = snooperInfo;
            this.ExportHTTPFacade = exportFTPFacade;
        }

        #region Overrides of ExportBaseViewModel
        public override Task PreRender()
        {
            if(!Context.IsPostBack)
            {
                this.Messages.PagingOptions.PageSize = 15;
                this.Messages.SortingOptions.SortDescending = false;
                this.Messages.SortingOptions.SortExpression = "Message.TimeStamp";
                this.Files.PagingOptions.PageSize = 15;
                this.Files.SortingOptions.SortDescending = false;
                this.Files.SortingOptions.SortExpression = "Message.TimeStamp";
                this.Images.PagingOptions.PageSize = 12;
                this.Images.SortingOptions.SortDescending = false;
                this.Images.SortingOptions.SortExpression = "Message.TimeStamp";
                this.ExportHTTPFacade.InitMessageFilter(this.MessageFilter, this.InvestigationId);
                this.ExportHTTPFacade.InitFileFilter(this.FileFilter, this.InvestigationId);
                this.ExportHTTPFacade.InitFileFilter(this.ImageFilter, this.InvestigationId);
                this.Clear("Messages");
                this.Clear("Files");
                this.Clear("Images");
                this.Messages.IsRefreshRequired = true;
                this.Files.IsRefreshRequired = true;
                this.Images.IsRefreshRequired = true;
            }

            if(this.Messages.IsRefreshRequired)
            {
                ExportHTTPFacade.FillMessages(this.InvestigationId, this.Messages, this.MessageFilter);
            }
            if (this.Files.IsRefreshRequired)
            {
                ExportHTTPFacade.FillFiles(this.InvestigationId, this.Files, this.FileFilter, this.AppPath);
            }
            if (this.Images.IsRefreshRequired)
            {
                ExportHTTPFacade.FillFiles(this.InvestigationId, this.Images, this.ImageFilter, this.AppPath, true);
            }

            return base.PreRender();
        }
        #endregion

        public void Filter(string type)
        {
            this.Messages.IsRefreshRequired = false;
            this.Files.IsRefreshRequired = false;
            this.Images.IsRefreshRequired = false;

            switch (type)
            {
                case "Messages":
                    this.Messages.IsRefreshRequired = true;
                    this.Messages.PagingOptions.PageIndex = 0;
                    break;
                case "Files":
                    this.Files.IsRefreshRequired = true;
                    this.Files.PagingOptions.PageIndex = 0;
                    break;
                case "Images":
                    this.Images.IsRefreshRequired = true;
                    this.Images.PagingOptions.PageIndex = 0;
                    break;
            }
        }

        public void Clear(string type)
        {
            switch (type)
            {
                case "Messages":
                    this.ClearFilter(this.MessageFilter);
                    this.Messages.PagingOptions.PageIndex = 0;
                    break;
                case "Files":
                    this.ClearFilter(this.FileFilter);
                    this.Files.PagingOptions.PageIndex = 0;
                    break;
                case "Images":
                    this.ClearFilter(this.ImageFilter);
                    this.Images.PagingOptions.PageIndex = 0;
                    break;
            }
            Filter(type);
        }

        private void ClearFilter(ExportFilterDTO filter)
        {
            filter.SearchText = "";
            filter.DurationTo = filter.DurationMax.ToString("dd.MM.yyyy HH:mm:ss");
            filter.DurationFrom = filter.DurationMin.ToString("dd.MM.yyyy HH:mm:ss");
        }

        public void UpdateInvestigationMenu()
        {
            this.Messages.IsRefreshRequired = false;
            this.Files.IsRefreshRequired = false;
            this.Images.IsRefreshRequired = false;
            base.UpdateInvestigationMenu();
        }


    }


}

