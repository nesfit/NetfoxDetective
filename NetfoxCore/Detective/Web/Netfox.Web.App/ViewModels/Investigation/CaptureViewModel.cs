using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.BusinessPack.Controls;
using DotVVM.Framework.Storage;
using DotVVM.Framework.ViewModel;
using Hangfire;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Facades;
using Newtonsoft.Json;

namespace Netfox.Web.App.ViewModels.Investigation
{
    public class CaptureViewModel : LayoutInvestigationViewModel
    {
        public override string Title => "Capture " + this.CaptureName;
        public BusinessPackDataSet<L3ConversationDTO> L3Convesations{ get; set; } = new BusinessPackDataSet<L3ConversationDTO>();
        public BusinessPackDataSet<L4ConversationDTO> L4Convesations{ get; set; } = new BusinessPackDataSet<L4ConversationDTO>();
        public BusinessPackDataSet<L7ConversationDTO> L7Convesations{ get; set; } = new BusinessPackDataSet<L7ConversationDTO>();
        public BusinessPackDataSet<PmFrameBaseDTO> Frames{ get; set; } = new BusinessPackDataSet<PmFrameBaseDTO>();
        [Bind(Direction.ServerToClientFirstRequest)]
        public ConvesationStatisticsDTO ConversationsStatistics { get; set; }
        [Bind(Direction.ServerToClientFirstRequest)]
        public BusinessPackDataSet<ProtocolSummaryItem> AppProtocols { get; set; } = new BusinessPackDataSet<ProtocolSummaryItem>();
        [Bind(Direction.ServerToClientFirstRequest)]
        public BusinessPackDataSet<ProtocolSummaryItem> TransportProtocols { get; set; } = new BusinessPackDataSet<ProtocolSummaryItem>();

        public string CaptureName { get; set; }

        [Bind(Direction.ServerToClientFirstRequest)]
        public string ChartLabels { get; set; }
        [Bind(Direction.ServerToClientFirstRequest)]
        public string ChartData { get; set; }
        [Bind(Direction.ServerToClientFirstRequest)]
        public string ChartColors { get; set; }

        public ConversationFilterDTO L3Filters { get; set; } = new ConversationFilterDTO();

        public ConversationFilterDTO L4Filters { get; set; } = new ConversationFilterDTO();

        public ConversationFilterDTO L7Filters { get; set; } = new ConversationFilterDTO();

        public FrameFilterDTO FrameFilters { get; set; } = new FrameFilterDTO();

        public List<string> SelectedSnoopers { get; set; } = new List<string>();

        public List<KeyValueDTO<string, string>> AvailableSnoopers { get; set; }

        public IList<ExportedProtocolsDTO> ExportedProtocols { get; set; }

        public bool IsCypherKeyDialogDisplayed { get; set; } = false;

        public UploadData CypherKeyData { get; set; } = new UploadData();

        [FromRoute("CaptureId")]
        public Guid CaptureId { get; set; }


        public CaptureViewModel(InvestigationFacade investigationFacade, HangfireFacade hangfireFacade, CaptureFacade captureFacade, ExportFacade exportFacade) : base(investigationFacade, hangfireFacade, captureFacade, exportFacade) {}

        #region Overrides of DotvvmViewModelBase
        public override Task PreRender()
        {
            var tmpColors = new List<string>()
            {
                "#3fa7c4",
                "#9ed552",
                "#e1db63",
                "#c49fd9",
                "#c5df5e",
                "#55c0cd",
                "#86d8a6",
                "#deb3b7"
            };


            if (!Context.IsPostBack)
            {
                this.L3Convesations.PagingOptions.PageSize = 15;
                this.L3Convesations.SortingOptions.SortDescending = false;
                this.L3Convesations.SortingOptions.SortExpression = nameof(L3ConversationDTO.FirstSeen);
                this.L4Convesations.PagingOptions.PageSize = 15;
                this.L4Convesations.SortingOptions.SortDescending = false;
                this.L4Convesations.SortingOptions.SortExpression = nameof(L4ConversationDTO.FirstSeen);
                this.L7Convesations.PagingOptions.PageSize = 15;
                this.L7Convesations.SortingOptions.SortDescending = false;
                this.L7Convesations.SortingOptions.SortExpression = nameof(L7ConversationDTO.FirstSeen);
                this.Frames.PagingOptions.PageSize = 15;
                this.Frames.SortingOptions.SortDescending = false;
                this.Frames.SortingOptions.SortExpression = nameof(PmFrameBaseDTO.FirstSeen);

                this.ConversationsStatistics = this.CaptureFacade.GetConversationStatistics(this.CaptureId, this.InvestigationId);
                this.SetupConversationFilter(L3Filters, "L3");
                this.SetupConversationFilter(L4Filters, "L4");
                this.SetupConversationFilter(L7Filters, "L7");
                this.SetupFrameFilter(FrameFilters);

                if (this.ConversationsStatistics != null)
                {

                    this.AppProtocols.LoadFromQueryable(this.ConversationsStatistics.AppProtocolsSummary.AsQueryable());
                    this.TransportProtocols.LoadFromQueryable(this.ConversationsStatistics.TransportProtocolsSummary.AsQueryable());

                    var labels = new List<string>();
                    var values = new List<float>();
                    var colors = new List<string>();

                    foreach (var p in this.ConversationsStatistics.AppProtocolsSummary)
                    {
                        labels.Add(p.Name);
                        values.Add(p.Percent);
                        colors.Add(tmpColors[colors.Count % tmpColors.Count]);
                    }

                    this.ChartLabels = JsonConvert.SerializeObject(labels);
                    this.ChartData = JsonConvert.SerializeObject(values);
                    this.ChartColors = JsonConvert.SerializeObject(colors);

                }
            }

            if (this.L3Convesations.IsRefreshRequired)
            {
                this.CaptureFacade.FillL3ConversationDataSet(this.L3Convesations, this.CaptureId, this.InvestigationId, this.L3Filters);
            }
            if (this.L4Convesations.IsRefreshRequired)
            {
                this.CaptureFacade.FillL4ConversationDataSet(this.L4Convesations, this.CaptureId, this.InvestigationId, this.L4Filters);
            }
            if (this.L7Convesations.IsRefreshRequired)
            {
                this.CaptureFacade.FillL7ConversationDataSet(this.L7Convesations, this.CaptureId, this.InvestigationId, this.L7Filters);
            }
            if (this.Frames.IsRefreshRequired)
            {
                this.CaptureFacade.FillPmFrameDataSet(this.Frames, this.CaptureId, this.InvestigationId, FrameFilters);
            }

            this.CaptureName = this.CaptureFacade.GetConversationStatistics(this.CaptureId, this.InvestigationId).Name;
            this.AvailableSnoopers = this.ExportFacade.GetSnooperList();
            var exportedProtocols = this.CaptureFacade.Stats.GetExportedProtocols(this.CaptureId);
            this.ExportedProtocols = new List<ExportedProtocolsDTO>();
            foreach (var p in exportedProtocols)
            {
                var text = p.Split('.').Last().Replace("Snooper", string.Empty);
                this.ExportedProtocols.Add(new ExportedProtocolsDTO(){Name = p, Text = text});
            }

            return base.PreRender();
        }
        #endregion

        public void Filter(string type)
        {
            this.L3Convesations.IsRefreshRequired = false;
            this.L4Convesations.IsRefreshRequired = false;
            this.L7Convesations.IsRefreshRequired = false;
            this.Frames.IsRefreshRequired = false;

            switch(type)
            {
                case "L3":
                    this.L3Convesations.IsRefreshRequired = true;
                    this.L3Convesations.PagingOptions.PageIndex = 0;
                    break;
                case "L4":
                    this.L4Convesations.IsRefreshRequired = true;
                    this.L4Convesations.PagingOptions.PageIndex = 0;
                    break;
                case "L7":
                    this.L7Convesations.IsRefreshRequired = true;
                    this.L7Convesations.PagingOptions.PageIndex = 0;
                    break;
                case "Frame":
                    this.Frames.IsRefreshRequired = true;
                    this.Frames.PagingOptions.PageIndex = 0;
                    break;
            }
        }

        public void Clear(string type)
        {
            switch (type)
            {
                case "L3":
                    this.ClearFilter(L3Filters);
                    this.L3Convesations.PagingOptions.PageIndex = 0;
                    break;
                case "L4":
                    this.ClearFilter(L4Filters);
                    this.L4Convesations.PagingOptions.PageIndex = 0;
                    break;
                case "L7":
                    this.ClearFilter(L7Filters);
                    this.L7Convesations.PagingOptions.PageIndex = 0;
                    break;
               case "Frame":
                    this.ClearFrameFilter(FrameFilters);
                   this.Frames.PagingOptions.PageIndex = 0;
                    break;
            }
            Filter(type);
        }

        private void ClearFilter(ConversationFilterDTO filter)
        {
            filter.SearchText = "";
            filter.BytesFrom = filter.BytesMin;
            filter.BytesTo = filter.BytesMax;
            filter.FramesFrom = filter.FrameMin;
            filter.FramesTo = filter.FrameMax;
            filter.DurationTo = filter.DurationMax.ToString("dd.MM.yyyy HH:mm:ss");
            filter.DurationFrom = filter.DurationMin.ToString("dd.MM.yyyy HH:mm:ss");
        }

        private void SetupConversationFilter (ConversationFilterDTO filter, string type)
        {
            filter.DurationMax = this.ConversationsStatistics.FilterDurationMax;
            filter.DurationMin = this.ConversationsStatistics.FilterDurationMin;
            switch (type)
            {
                   
                case "L3":
                    filter.FrameMax = this.ConversationsStatistics.L3FramesMax;
                    filter.FrameMin = this.ConversationsStatistics.L3FramesMin;
                    filter.BytesMax = this.ConversationsStatistics.L3BytesMax;
                    filter.BytesMin = this.ConversationsStatistics.L3BytesMin;

                    break;
                case "L4":
                    filter.FrameMax = this.ConversationsStatistics.L4FramesMax;
                    filter.FrameMin = this.ConversationsStatistics.L4FramesMin;
                    filter.BytesMax = this.ConversationsStatistics.L4BytesMax;
                    filter.BytesMin = this.ConversationsStatistics.L4BytesMin;
                    break;
                case "L7":
                    filter.FrameMax = this.ConversationsStatistics.L4FramesMax;
                    filter.FrameMin = this.ConversationsStatistics.L4FramesMin;
                    filter.BytesMax = this.ConversationsStatistics.L4BytesMax;
                    filter.BytesMin = this.ConversationsStatistics.L4BytesMin;
                    break;

            }
            ClearFilter(filter);
        }

        private void SetupFrameFilter(FrameFilterDTO filter)
        {
            filter.DurationMax = this.ConversationsStatistics.FilterDurationMax;
            filter.DurationMin = this.ConversationsStatistics.FilterDurationMin;
            filter.BytesMax = this.ConversationsStatistics.FramesBytesMax;
            filter.BytesMin = this.ConversationsStatistics.FramesBytesMin;
            ClearFrameFilter(filter);
        }

        private void ClearFrameFilter(FrameFilterDTO filter)
        {
            filter.SearchText = "";
            filter.BytesFrom = filter.BytesMin;
            filter.BytesTo = filter.BytesMax;
            filter.DurationTo = filter.DurationMax.ToString("dd.MM.yyyy HH:mm:ss");
            filter.DurationFrom = filter.DurationMin.ToString("dd.MM.yyyy HH:mm:ss");
        }

        public void ProcessKey()
        {
            var fileStorage = this.Context.Configuration.ServiceLocator.GetService<IUploadedFileStorage>();
            var path = Path.Combine(this.AppPath, "tmp");

            // save all files to disk
            foreach (var file in this.Upload.Files)
            {
                var paths = Path.Combine(path, file.FileId.ToString());
                fileStorage.SaveAs(file.FileId, Path.Combine(path, file.FileName));
                fileStorage.DeleteFile(file.FileId);

                var pk = File.ReadAllText(Path.Combine(path, file.FileName));
                File.Delete(Path.Combine(path, file.FileName));

                this.CaptureFacade.AddCypherKey(this.InvestigationId, this.CaptureId, pk);

            }

            // clear the data so the user can continue with other files
            this.Upload.Clear();
            this.IsCypherKeyDialogDisplayed = false;
        }

        public string Export()
        {
            var jobId = BackgroundJob.Enqueue(() => this.HangfireFacade.ExportData(this.InvestigationId, this.SelectedSnoopers, this.AppPath, this.CaptureId));
            var investigation = this.InvestigationFacade.GetDetail(this.InvestigationId);
            investigation.Jobs.Add(jobId);
            var investigatorIds = investigation.UserInvestigations.Select(ui => ui.UserId).ToList();
            this.InvestigationFacade.Save(investigation, investigatorIds);
            return jobId;
        }

        public void RemoveExportProtocol(string protocol)
        {
            this.CaptureFacade.Stats.RemoveExportedProtocol(this.CaptureId, protocol);
        }

        public void UpdateInvestigationMenu()
        {
            this.L3Convesations.IsRefreshRequired = false;
            this.L4Convesations.IsRefreshRequired = false;
            this.L7Convesations.IsRefreshRequired = false;
            this.Frames.IsRefreshRequired = false;
            base.UpdateInvestigationMenu();
        }
    }
}
