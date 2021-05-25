using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Castle.Windsor;
using GalaSoft.MvvmLight.Command;
using Netfox.Core.Helpers;
using Netfox.Detective.Models.Conversations;
using Netfox.Detective.Services;
using Netfox.Detective.ViewModelsDataEntity.Exports;
using Netfox.Framework.Models.Interfaces;

namespace Netfox.Detective.ViewModelsDataEntity.ConversationsCollections
{
    public class ConversationsGroupVm: ConversationsVm<IConversationsModel>
    {
        private readonly object _allExportResultsLock = new object();
        private readonly object _initLock = new object();
        private NotifyTaskCompletion<ExportVm[]> _AllExportResults;

        private IEnumerable<CaptureVm> _captures;

        public ConversationsGroupVm(WindsorContainer applicationWindsorContainer, IConversationsModel model, ExportService exportService) : base(applicationWindsorContainer, model,exportService)
        {
            if(model.GetType() == typeof(ConversationsGroup)) {
                this.ConversationsGroup = model as ConversationsGroup;
            }
            else
            {
                this.ConversationsGroup = new ConversationsGroup(model);
            }
        }

        public ICommand CGetCaptureDetails => new RelayCommand(this.CreateGroupStats);

        public ConversationsGroup ConversationsGroup { get; set; }

        public IEnumerable<CaptureVm> Captures
        {
            set { this._captures = value; }
        }

        public new ExportVm[] AllExportResults
        {
            get
            {
                lock(this._allExportResultsLock)
                {
                    return this._AllExportResults
                           ?? (this._AllExportResults =
                               new NotifyTaskCompletion<ExportVm[]>(() => Task.Run(() => this.FindAllExportResults().ToArray()),
                                   () => this.OnPropertyChanged(nameof(this.AllExportResults)), false));
                }
            }
        }

        public void CreateGroupStats()
        {
            //var bgTask = this.BgTasksManagerService.CreateTask("Group statistics", this.ConversationsGroup.ToString());
            //todo bgTask.Start(this.ConversationsGroup.CreateStats);
        }

        private IEnumerable<ExportVm> FindAllExportResults()
        {
            foreach(var conversationVm in this.L7Conversations)
            {
                foreach(var result in conversationVm.AllExportResults) { yield return result; }
            }
        }

        //    if(this.Initialized) { return; }
        //{
        //public override void Init()

        //#region Initialization
        //{

        //public IConversationsModel ConversationsGroup
        //   public new ViewModelsObservableCollection<ConversationVm, Conversation> Conversations { get; set; }

        //    lock(this._initLock)
        //    {
        //        if(this.Initialized) { return; }

        //        if(this._captures != null)
        //        {
        //            var conversationsVms = new ConversationVm[this.Conversations.Count()];

        //            //var capturesVmsById = this._captures.ToDictionary(captureVm => captureVm.EncapsulatedData.Id,
        //            //    captureVm => new KeyValue<CaptureVm, Dictionary<string, ConversationVm>>(captureVm, null));

        //            //Parallel.ForEach(this.EncapsulatedData.Conversations, (conversation, state, index) =>
        //            //{
        //            //    KeyValue<CaptureVm, Dictionary<string, ConversationVm>> capture;
        //            //    if(capturesVmsById.TryGetValue(conversation.CaptureId, out capture))
        //            //    {
        //            //        capture.Key.Init();
        //            //        if(capture.Value == null) {
        //            //            lock(capture) { if(capture.Value == null) { capture.Value = capture.Key.Conversations.ToDictionary(c => c.Conversation.Id); } }
        //            //        }

        //            //        ConversationVm conversationVm;
        //            //        if(capture.Value.TryGetValue(conversation.Id, out conversationVm)) { conversationsVms[index] = conversationVm; }
        //            //    }
        //            //});

        //            this.Conversations.SetViewModelsExternal(conversationsVms);

        //            //this.ConversationsGroup.InitFrames();

        //            this.Initialized = true;
        //        }
        //    }
        //}
        //#endregion
        //    get { return this.Conversations.; }
        //}
    }
}