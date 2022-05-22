using System.Windows;
using System.Windows.Input;
using Castle.Windsor;
using GalaSoft.MvvmLight.Command;
using Netfox.Detective.Interfaces;
using Netfox.Detective.Messages.Captures;
using Netfox.Detective.Messages.Conversations;
using Netfox.Detective.Services;
using Netfox.Detective.ViewModels.Frame;
using Netfox.Detective.ViewModels.Investigations;
using Netfox.Framework.Models.PmLib.Captures;

namespace Netfox.Detective.ViewModelsDataEntity.ConversationsCollections
{
    public class CaptureVm : ConversationsVm<PmCaptureBase>
    {
        private readonly IDetectiveMessenger _messenger;

        #region Constructor

        public CaptureVm(IWindsorContainer applicationWindsorContainer, PmCaptureBase model,
            ExportService exportService) : base(applicationWindsorContainer, model, exportService)
        {
            this.Capture = model;
            //this.Conversations.NewVmCreated += this.ConversationsOnNewVmCreated;

            this._messenger = this.ApplicationOrInvestigationWindsorContainer.Resolve<IDetectiveMessenger>();
        }

        #endregion

        public PmCaptureBase Capture { get; set; }

        #region Members

        #endregion

        #region Initialization

        //public override void Init()
        //{
        //    if(this.Initialized) { return; }

        //    lock(this._initLock)
        //    {
        //        if(this.Initialized) { return; }
        //        try
        //        {
        //            this.Capture.InitFrames();
        //            this.Conversations.Activate(true);
        //        }
        //        catch(PmCaptureManager.UnknownFileType ex) {
        //            this.SystemServices.ShowMessageBox(ex.Message, "UnknownFileType", MessageBoxButton.OK, MessageBoxImage.Error);
        //        }
        //        this.Initialized = true;
        //        this.IsLoading = false;
        //    }
        //}

        #endregion

        #region External events handlers

        // private void ConversationsOnNewVmCreated(ConversationVm newViewModel) { newViewModel.ExportResultsProvider = this.ExportResultsProvider; }

        #endregion

        #region Capture actions

        public void CreateCaptureStats()
        {
            //var bgTask = this.BgTasksManagerService.CreateTask("Capture statistics", this.Capture.FileInfo.Name);
            //bgTask.Start(this.EncapsulatedData.CreateStats);
        }

        #region Frames navigation

        //public void SelectFrame(uint frameIndex)
        //{
        //	//var fvm = Array.Find(this.EncapsulatedData.Frames, f => f.FrameIndex == frameIndex);
        //	var fvm = this.Capture.Frames.First(f => f.FrameIndex == frameIndex);
        //	if(fvm != null) { this.SelectFrame(fvm); }
        //}

        //public void SelectFrame(PmFrameBase currentFrameVm)
        //{
        //	//var conversationVm = this.Conversations.FirstOrDefault(c => c.Conversation.Frames.Contains(currentFrameVm));
        //	//if(conversationVm != null)
        //	//{
        //	//    this.CurrentConversation = conversationVm;
        //	//    this.CurrentConversation.CurrentPacket = currentFrameVm;
        //	//}
        //}

        public void SelectNextFrame(FrameVm currentFrameVm)
        {
            //var cIndex = Array.IndexOf(this.EncapsulatedData.Frames, currentFrameVm);
            //if(cIndex < this.EncapsulatedData.Frames.Length - 1) { this.SelectFrame(this.EncapsulatedData.Frames[cIndex + 1]); }
        }

        public void SelectPrevFrame(FrameVm currentFrameVm)
        {
            //var cIndex = Array.IndexOf(this.EncapsulatedData.Frames, currentFrameVm);
            //if(cIndex > 0) { this.SelectFrame(this.EncapsulatedData.Frames[cIndex - 1]); }
        }

        #endregion

        #endregion

        #region Commands

        #region AddCaptureToExportList

        private void AddCaptureToExportList()
        {
            this._messenger.AsyncSend(new AddConversationToExportMessage
            {
                ConversationVm = this.CurrentConversation,
                BringToFront = false
            });
        }

        public ICommand CAddCaptureToExportList => new RelayCommand(this.AddCaptureToExportList);

        #endregion

        #region Create capture stistics

        private void CGetCaptureDetailsExecute() => this.CreateCaptureStats();

        //private bool CanCGetCaptureDetailsExecute() => ((this.Capture.IsChecksumCorrect == null || (bool) this.Capture.IsChecksumCorrect));

        public ICommand CGetCaptureDetails => new RelayCommand(this.CGetCaptureDetailsExecute);

        #endregion

        public void ProtocolsRecognition()
        {
            //ShowProtocolsRecognitionMessage.SendShowProtocolsRecognitionMessage(new ProtocolsRecognitionContext
            //{
            //    Controller = this.Capture.FwControllerContext,
            //    RecognitionScope = ProtocolsRecognitionContext.RecognitionScopeType.Capture,
            //    Capture = this.Capture.FwCaptureContext,
            //    CaptureScope = this.Capture
            //});
        }

        #region Protocols recognition

        public RelayCommand CProtocolsRecognition => new RelayCommand(this.ProtocolsRecognition);

        #endregion

        private void RemoveCapture(RoutedEventArgs e)
        {
            var frameworkElement = e.OriginalSource as FrameworkElement;
            if (frameworkElement == null)
            {
                return;
            }

            var capture = frameworkElement.DataContext as CaptureVm;

            var investigationExplorerVm =
                this.ApplicationOrInvestigationWindsorContainer.Resolve<InvestigationExplorerVm>();

            if (investigationExplorerVm != null && capture != null)
            {
                investigationExplorerVm.InvestigationVm.RemoveCaptureAsync(capture).Wait();
            }
        }

        public ICommand RemoveCaptureClick => new RelayCommand<RoutedEventArgs>(this.RemoveCapture);

        private void AddCaptureToExport(RoutedEventArgs e)
        {
            var frameworkElement = e.OriginalSource as FrameworkElement;
            if (frameworkElement == null)
            {
                return;
            }

            var capture = frameworkElement.DataContext as CaptureVm;

            var investigationExplorerVm =
                this.ApplicationOrInvestigationWindsorContainer.Resolve<InvestigationExplorerVm>();

            if (investigationExplorerVm.InvestigationVm != null && capture != null)
            {
                this._messenger.AsyncSend(new AddCaptureToExportMessage
                {
                    CaptureVm = capture,
                    BringToFront = true
                });
            }
        }

        public ICommand AddCaptureToExportClick => new RelayCommand<RoutedEventArgs>(this.AddCaptureToExport);

        #endregion

        #region Properties

        private object _exportResultsLock = new object();

        //public new ExportVm[] AllExportResults
        //{
        //    get
        //    {
        //        if(this.ExportResultsProvider == null) { return null; }

        //        return this.Cache.GetValueOrSetAsync(CachedItems.AllExportResults, () =>
        //        {
        //            //var allExportResults = this.ExportResultsProvider.ExportResults(result => ResultState.CaptureId == this.EncapsulatedData.Id).ToArray();
        //            var allExportResults = this.ExportResultsProvider.ExportResults(result => true).ToArray(); //todo fix
        //            return allExportResults;
        //        }, task => this.OnPropertyChanged(), 0, this._exportResultsLock) as ExportVm[];
        //    }
        //}

        public string Sha1Checksum => this.Capture.PcapHash;

        #endregion
    }
}