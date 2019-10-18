using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotVVM.BusinessPack.Controls;
using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.Storage;
using DotVVM.Framework.ViewModel;
using Hangfire;
using Netfox.Core.Interfaces;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Facades;

namespace Netfox.Web.App.ViewModels.Investigation
{
    [Authorize]
    public class LayoutInvestigationViewModel : MasterpageViewModel
    {
        public override string Title => "";

        public override string ColumnName => this.InvestigationName;

        public override bool ShowToolbar => false;

        public override string ColumnCSSClass => "wrapper-settings investigation";

        public string InvestigationName { get; set; }

        protected readonly InvestigationFacade InvestigationFacade;

        protected readonly HangfireFacade HangfireFacade;

        protected readonly CaptureFacade CaptureFacade;

        protected readonly ExportFacade ExportFacade;

        [FromRoute("InvestigationId")]
        public Guid InvestigationId { get; private set; }

        public bool IsUploadDialogDisplayed { get; set; } = false;

        public UploadData Upload { get; set; } = new UploadData();

        public int RefreshCounter { get; set; } = 0;

        public IEnumerable<CaptureDTO> CaptureList { get; set; } 

        [Bind(Direction.ServerToClient)]
        public List<KeyValueDTO<string, string>> ExportList { get; set; } = new List<KeyValueDTO<string, string>>();

        public LayoutInvestigationViewModel(InvestigationFacade investigationFacade, HangfireFacade hangfireFacade, CaptureFacade captureFacade, ExportFacade exportFacade)
        {
            this.InvestigationFacade = investigationFacade;
            this.HangfireFacade = hangfireFacade;
            this.CaptureFacade = captureFacade;
            this.ExportFacade = exportFacade;
        }

        private string GetFolderPath()
        {
            var folderPath = Path.Combine(this.Context.Configuration.ApplicationPhysicalPath, "tmp");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            return folderPath;
        }

        public void Process()
        {
            var folterPath = this.GetFolderPath();
            var fileStorage = this.Context.Configuration.ServiceLocator.GetService<IUploadedFileStorage>();
            var id = Guid.Parse(this.Context.Parameters["InvestigationId"].ToString());
           
            // save all files to disk
            foreach (var file in this.Upload.Files)
            {
                var filePath = Path.Combine(folterPath, file.FileName );
                fileStorage.SaveAs(file.FileId, filePath);
                fileStorage.DeleteFile(file.FileId);

                this.StartProcessCapture(InvestigationId, filePath);
            }

            // clear the data so the user can continue with other files
            this.Upload.Clear();
            this.IsUploadDialogDisplayed = false;
        }

        public void UpdateInvestigationMenu() { this.RefreshCounter++; }

        public override Task PreRender()
        {
            var investigation = this.InvestigationFacade.GetDetail(this.InvestigationId);

            this.InvestigationName = investigation.Name;

            this.CaptureList = this.CaptureFacade.GetCaptureList(this.InvestigationId);
            var exportRoute = this.Context.Configuration.RouteTable
                .Where(r => Regex.IsMatch(r.VirtualPath, @"Views/Investigation/Export(/Framework/Snooper(\w)*.WEB)?/Export(\p{L}|\d)+.dothtml")).ToList();
            var paramRoute = new Dictionary<string, object>();
            paramRoute.Add("InvestigationId", this.InvestigationId);
            
            foreach (var route in exportRoute)
            {
                var name = Regex.Replace(route.RouteName, @"Investigation_\{InvestigationId:guid\}_Export_(Framework_Snooper(\w)*.WEB_)?Export", String.Empty);
                this.ExportList.Add(new KeyValueDTO<string, string>(name, route.BuildUrl(paramRoute).Remove(0, 1)));
            }
               
            this.InvestigationFacade.UpdateLastAccess(InvestigationId, this.Username);
            return base.PreRender();
        }

        protected string StartProcessCapture(Guid investigationId, string filePath)
        {
            var jobId = BackgroundJob.Enqueue(() => this.HangfireFacade.ProcessCapture(investigationId, this.AppPath, filePath));
            var investigation = this.InvestigationFacade.GetDetail(this.InvestigationId);
            investigation.Jobs.Add(jobId);
            var investigatorIds = investigation.UserInvestigations.Select(ui => ui.UserId).ToList();
            this.InvestigationFacade.Save(investigation, investigatorIds);
            return jobId;
        }
    }
}
