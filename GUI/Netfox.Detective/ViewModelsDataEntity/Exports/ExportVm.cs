// Copyright (c) 2017 Jan Pluskal, Martin Mares, Martin Kmet
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using Castle.Windsor;
using GalaSoft.MvvmLight.Command;
using Netfox.Core.Helpers;
using Netfox.Core.Interfaces.Model.Exports;
using Netfox.Core.Interfaces.ViewModels;
using Netfox.Detective.ViewModelsDataEntity.Exports.Detail;
using Netfox.Detective.ViewModelsDataEntity.Exports.ModelWrappers;
using Netfox.Framework.Models.Snoopers;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModelsDataEntity.Exports
{
    //TODO implement all
    [NotifyPropertyChanged]
    public class ExportVm : DetectiveDataEntityViewModelBase, IDataEntityVm
    {
        //public string Description
        //{
        //    get { return this.Export.Description; }
        //}

        //public string ExporterType
        //{
        //    get { return this.Export.ExporterTypeName; }
        //}

        //public string ResultType // TODO
        //{
        //    get { return ExportsFactory.SpecificResultTypeName(this.Export.ExporterType); }
        //}

        private readonly object _durationLock = new object();

        private readonly object _exportReportsLock = new object();

        private readonly object _periodLock = new object();

        private readonly object _timeStampLock = new object();
        private NotifyTaskCompletion<TimeSpan> _duration;
        private NotifyTaskCompletion<ExportReportVm[]> _ExportReports;
        private NotifyTaskCompletion<string> _Period;
        private SnooperExportedObjectBase _selectedSnooperExportObject;
        private NotifyTaskCompletion<DateTime> _TimeStamp;

        public ExportVm(WindsorContainer investigationWindsorContainer, SnooperExportBase model) : base(investigationWindsorContainer, model)
        {
            this.Export = model;
            //if (this.Export.ExportObjects == null) //no exported objects present
            //this.SnooperExportedObjects = new ViewModelsIoCObservableCollection<SnooperExportedObjectBaseVm, SnooperExportedObjectBase>(new List<SnooperExportedObjectBase>(), this.InvestigationOrAppWindsorContainer);
            //else this.SnooperExportedObjects = new ViewModelsIoCObservableCollection<SnooperExportedObjectBaseVm, SnooperExportedObjectBase>(this.Export.ExportObjects, this.InvestigationOrAppWindsorContainer);
            //data.ExportResultUpdated += this.Data_ExportResultUpdated;
        }

        [IgnoreAutoChangeNotification]
        public IEnumerable<SnooperExportedObjectBase> SnooperExportedObjects => this.Export.ExportObjects;

        public SnooperExportBase Export { get; }

        public bool IsGroup => false;

        public string Name
        {
            get
            {
                // if (!string.IsNullOrEmpty(this.Export.Name)) { return this.Export.InvestigationName; }
                /*
                if (!String.IsNullOrEmpty(Data.Description))
                    return String.Format("{0} {1}", ResultType, Data.Description);*/

                //return String.Format("{0} {1}>{2}", this.ResultType, this.ClientAddress, this.ServerAddress);
                return string.Empty;
            }
        }

        [IgnoreAutoChangeNotification]
        public NotifyTaskCompletion<TimeSpan> Duration
        {
            get
            {
                lock(this._durationLock)
                {
                    return this._duration
                           ?? (this._duration =
                               new NotifyTaskCompletion<TimeSpan>(() => Task.Run(() => this.GetDuration()), () => this.OnPropertyChanged(nameof(this.Duration)), false));
                }
            }
        }

        [IgnoreAutoChangeNotification]
        public DateTime TimeStamp
        {
            get
            {
                lock(this._timeStampLock)
                {
                    return this._TimeStamp
                           ?? (this._TimeStamp =
                               new NotifyTaskCompletion<DateTime>(() => Task.Run(() => this.GetTimeStamp()), () => this.OnPropertyChanged(nameof(this.TimeStamp)), false));
                }
            }
        }

        [IgnoreAutoChangeNotification]
        public int EventsCount => this.Export?.ExportObjects?.Count ?? 0;

        [IgnoreAutoChangeNotification]
        public string Period
        {
            get
            {
                lock(this._periodLock)
                {
                    return this._Period
                           ?? (this._Period = new NotifyTaskCompletion<string>(() => Task.Run(() => this.GetPeriod()), () => this.OnPropertyChanged(nameof(this.Period)), false));
                }
            }
        }

        [IgnoreAutoChangeNotification]
        public IPEndPoint SourceEndPoint => this.Export?.ExportSource?.SourceEndPoint;

        [IgnoreAutoChangeNotification]
        public IPEndPoint DestinationEndPoint => this.Export?.ExportSource?.DestinationEndPoint;

        public SnooperExportedObjectBase SelectedSnooperExportObject
        {
            get { return this._selectedSnooperExportObject; }
            set
            {
                this._selectedSnooperExportObject = value;
                this.OnPropertyChanged();
            }
        }

        [IgnoreAutoChangeNotification]
        public IEnumerable<CallVm> Calls => this.SnooperExportedObjects.Where(i => i is ICall).Select(call => this.ApplicationOrInvestigationWindsorContainer.Resolve<CallVm>(new
        {
            model = call,
            investigationOrAppWindsorContainer = this.ApplicationOrInvestigationWindsorContainer,
            exportVm = this
        }));

        [IgnoreAutoChangeNotification]
        public IEnumerable<EmailVm> Emails
            => this.SnooperExportedObjects.Where(i => i is IEMail).Select(email => this.ApplicationOrInvestigationWindsorContainer.Resolve<EmailVm>(new
            {
                model = email,
                investigationOrAppWindsorContainer = this.ApplicationOrInvestigationWindsorContainer,
                exportVm = this
            }));

        [IgnoreAutoChangeNotification]
        public IEnumerable<ChatMessageVm> ChatMessages
            => this.SnooperExportedObjects.Where(i => i is IChatMessage).Select(chatMessage => this.ApplicationOrInvestigationWindsorContainer.Resolve<ChatMessageVm>(new
            {
                model = chatMessage,
                investigationOrAppWindsorContainer = this.ApplicationOrInvestigationWindsorContainer,
                exportVm = this
            }));

        [IgnoreAutoChangeNotification]
        public IEnumerable<IChatGroupMessage> ChatGroupMessages => this.SnooperExportedObjects.Where(i => i is IChatGroupMessage).Cast<IChatGroupMessage>();

        //[IgnoreAutoChangeNotification]
        //public IEnumerable<IChatMessage> ChatMessages => this.SnooperExportedObjects.Where(i => i is IChatMessage).Cast<IChatMessage>();

        [IgnoreAutoChangeNotification]
        public IEnumerable<IFileMessage> FileMessages => this.SnooperExportedObjects.Where(i => i is IFileMessage).Cast<IFileMessage>();

        [IgnoreAutoChangeNotification]
        public IEnumerable<IPhotoMessage> PhotoMessages => this.SnooperExportedObjects.Where(i => i is IPhotoMessage).Cast<IPhotoMessage>();

        [IgnoreAutoChangeNotification]
        public IEnumerable<KeyValuePair<IChatMessage, IEnumerable<IChatMessage>>> ChatConversations
            =>
                this.ChatMessages.GroupBy(msg => msg.ChatMessage, new ChatMessageComparer())
                    .Select(conv => new KeyValuePair<IChatMessage, IEnumerable<IChatMessage>>(conv.Key, conv.Select(c => c.ChatMessage)));

        public KeyValuePair<IChatMessage, IEnumerable<IChatMessage>> SelectedChatConversation { get; set; }

        [IgnoreAutoChangeNotification]
        public int ExportObjectsCount => this.Export.ExportObjects.Count;

        [IgnoreAutoChangeNotification]
        public int TotalExportReportsCount => this.Export.Reports.Count;

        [IgnoreAutoChangeNotification]
        public ExportReportVm[] ExportReports
        {
            get
            {
                lock(this._periodLock)
                {
                    return this._ExportReports
                           ?? (this._ExportReports =
                               new NotifyTaskCompletion<ExportReportVm[]>(() => Task.Run(() => this.GetExportReports()), () => this.OnPropertyChanged(nameof(this.ExportReports)),
                                   false));
                }
            }
        }

        [IgnoreAutoChangeNotification]
        //public ICommand CDelete => new RelayCommand(this.Export.PersistenceDelete);
        public ICommand CDelete => new RelayCommand(() =>{this.Logger?.Info("Export delete not implemented.");});

        [IgnoreAutoChangeNotification]
        public ICommand CSelectConversation => null; //new RelayCommand(this.SelectConversation, this.CanSelectConversationExecute);

        [IgnoreAutoChangeNotification]
        public ICommand CSelectCapture => null;

        [IgnoreAutoChangeNotification]
        public string ExporterType => this.Export.GetType().Name;

        //public SnooperExportedObjectBaseVm SelectedDataUser
        //{
        //    get { return this.SelectedSnooperExportObject; }
        //    set
        //    {
        //        this.SelectedSnooperExportObject = value;
        //        this.OnPropertyChanged();

        //        Task.Factory.StartNew(() =>
        //        {
        //            if (this.SelectedSnooperExportObject != null)
        //            {
        //                //this.SelectedData.Init();
        //                ExportDataMessage.SendExportDataMessage(this.SelectedSnooperExportObject, ExportDataMessage.MessageType.DataSelectedUser);
        //            }
        //        });
        //    }
        //}

        //public void SelectData(SnooperExportedObjectBaseVm data, bool userSelect)
        //{
        //    this.SelectedSnooperExportObject = data;
        //    ExportDataMessage.SendExportDataMessage(data, userSelect ? ExportDataMessage.MessageType.DataSelectedUser : ExportDataMessage.MessageType.DataSelected);
        //}

        //public void SelectData(SnooperExportedObjectBase data, bool userSelect)
        //{
        //    var exportDataVm = this.SnooperExportedObjects.FirstOrDefault(d => d.Equals(data));
        //    this.SelectData(exportDataVm, userSelect);
        //}

        public void SelectDataByDataObject(object obj, bool userSelect)
        {
            this.Logger?.Error($"SelectDataByDataObject is not implemented");
        }

        public void Updated()
        {
            this.OnPropertyChanged(nameof(Name));
            this.OnPropertyChanged(nameof(TimeStamp));
            this.OnPropertyChanged(nameof(EventsCount));
            this.OnPropertyChanged(nameof(Period));
            this.OnPropertyChanged(nameof(SourceEndPoint));
            this.OnPropertyChanged(nameof(DestinationEndPoint));
        }

        //private void SelectConversation()
        //{
        //    ConversationMessage.SendConversationMessage(this.Export.CaptureId, this.Export.ConversationIndex,
        //        ConversationMessage.MessageType.CurrentConversationChangedByIndex, true);
        //}

        private bool CanSelectConversationExecute() { return true; }

        private TimeSpan GetDuration()
        {
            var firstTimeStamp = this.Export.TimeStampFirst;
            var lastTimeStamp = this.Export.TimeStampLast;

            if(firstTimeStamp != DateTime.MinValue && lastTimeStamp != DateTime.MaxValue) { return firstTimeStamp - lastTimeStamp; }
            return TimeSpan.Zero;
        }

        private ExportReportVm[] GetExportReports()
        {
            var resultArray = this.Export.Reports?.Select(exportReport => new ExportReportVm(exportReport, this, this.ApplicationOrInvestigationWindsorContainer)).ToArray();
            return resultArray;
        }

        private string GetPeriod()
        {
            var firstTimeStamp = this.Export.TimeStampFirst;
            var lastTimeStamp = this.Export.TimeStampLast;

            return $"{firstTimeStamp} - {lastTimeStamp}";
        }

        private DateTime GetTimeStamp()
        {
            var first = this.Export.ExportObjects.Where(e => e != null).OrderBy(e => e.TimeStamp).FirstOrDefault();
            if(first == null) { return DateTime.MinValue; }
            return first.TimeStamp;
        }

        private enum CachedItems
        {
            ExportReports,
            TimeStamp,
            Period,
            Duration
        }

        //#endregion
        //}
        //    database.Insert(this.Export, true, true);
        //    this.Export.ExportGroupId = newParentGroup.ExportGroup.Id;
        //    this.Export.PersistenceProvider.Delete(this.Export, true, true);

        //    var database = this.Export.PersistenceProvider;
        //    if (newParentGroup == null || newParentGroup.ExportGroup.Id == this.Export.ExportGroupId) { return; }
        //{

        //public void MoveResult(ExportGroupVm newParentGroup)
        //}
        //        bringToFront);
        //    FrameMessage.SendFrameMessage(this.Export.CaptureId, this.Export.ConversationIndex, frameId, FrameMessage.MessageType.CurrentFrameByCaptureIdAndConvIndex,
        //{
        //public void SelectFrame(uint frameId, bool bringToFront)
        //  private ViewModelsIoCObservableCollection<SnooperExportedObjectBaseVm, SnooperExportedObjectBase> _exportData;
        //}
        //    }
        //        this.Updated();
        //        this.Export.Init();
        //        this.ExportedData.Activate();
        //        this.Initialized = true;

//new RelayCommand(()=> CaptureMessage.SendCaptureMessage(null, this.Export.CaptureId, CaptureMessage.MessageType.CurrentCaptureChangedById, true));

        // this.SelectData(this.SnooperExportedObjects.FirstOrDefault(d => d.Equals(obj)), userSelect);
        //   public ViewModelsIoCObservableCollection<SnooperExportedObjectBaseVm, SnooperExportedObjectBase> SnooperExportedObjects { get; }

        //#region Helper methods
        //public override void Init()
        //{
        //    if (!this.Initialized)

        //    {
    }
}