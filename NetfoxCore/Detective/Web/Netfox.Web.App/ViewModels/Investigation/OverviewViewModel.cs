using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using Hangfire;
using Hangfire.Storage;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Facades;

namespace Netfox.Web.App.ViewModels.Investigation
{
    public class OverviewViewModel : LayoutInvestigationViewModel
    {
       
        public override string Title => "Investigation Overview";

        public List<string> SelectedSnoopers { get; set; } = new List<string>();

        public List<KeyValueDTO<string, string>> AvailableSnoopers { get; set; }

        public InvestigationDTO Investigation { get; set; }
        
        public InvestigationStatisticsDTO Stats { get; set; } = new InvestigationStatisticsDTO();

        public OverviewViewModel(InvestigationFacade investigationFacade, HangfireFacade hangfireFacade, CaptureFacade captureFacade, ExportFacade exportFacade) : base(investigationFacade, hangfireFacade, captureFacade, exportFacade) {}

        #region Overrides of LayoutInvestigationViewModel
        public override Task PreRender()
        {
            this.Investigation = this.InvestigationFacade.GetDetail(this.InvestigationId);
            this.CaptureFacade.Stats.FillInvestigationStats(this.Stats, this.InvestigationId);
            this.Investigation.Owner = this.InvestigationFacade.GetOwner(this.InvestigationId);

            this.AvailableSnoopers = this.ExportFacade.GetSnooperList();

            this.Stats.TotalSizeText = this.GetSize(this.Stats.TotalSize);

            this.Stats.InProgressCapture = 0;
            this.Stats.InProgressExport = 0;
            this.Stats.FinishedCapture = 0;
            this.Stats.FinishedExport = 0;

            foreach (var jobId in Investigation.Jobs)
            {
                var job = JobStorage.Current.GetConnection().GetJobData(jobId);
                if(job != null)
                {
                    switch (job.Job.Method.Name)
                    {
                        case "ProcessCapture":
                            if (job.State == "Succeeded") { this.Stats.FinishedCapture++; }
                            else { this.Stats.InProgressCapture++; }
                            break;
                        case "ExportData":
                            if (job.State == "Succeeded") { this.Stats.FinishedExport++; }
                            else { this.Stats.InProgressExport++; }
                            break;
                    }
                }
            }
            return base.PreRender();
        }
        #endregion

        public string Export()
        {
            
            var jobId = BackgroundJob.Enqueue(() => this.HangfireFacade.ExportData(this.InvestigationId, this.SelectedSnoopers, this.AppPath));
            var investigation = this.InvestigationFacade.GetDetail(this.InvestigationId);
            investigation.Jobs.Add(jobId);
            var investigatorIds = investigation.UserInvestigations.Select(ui => ui.UserId).ToList();
            this.InvestigationFacade.Save(investigation, investigatorIds);
            return jobId;
        }

        public string GetSize(long size)
        {
            var sufix = new string[] {"B", "kB","MB","GB","TB","PB"};

            var index = (size > 0) ? ((int) (Math.Log(size, 2) / 10)) : 0;

            var newSize = (size > 0) ? (Math.Round(size / Math.Pow(2, index*10))) : 0;

            return $"{newSize} {sufix[index]}";
        }

    }
}
