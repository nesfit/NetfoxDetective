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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Castle.Windsor;
using GalaSoft.MvvmLight.Command;
using Netfox.Core.Collections;
using Netfox.Core.Helpers;
using Netfox.Core.Interfaces.ViewModels;
using Netfox.Core.Messages.Exports;
using Netfox.Detective.Models.Base;
using Netfox.Detective.Models.Exports;
using Netfox.Detective.ViewModels.CommunicationNetwork;
using Netfox.Detective.ViewModelsDataEntity.Exports.ModelWrappers;
using Netfox.Framework.Models.Snoopers;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModelsDataEntity.Exports
{
    [NotifyPropertyChanged]
    public class ExportGroupVm : DetectiveDataEntityViewModelBase, IDataEntityVm
    {
        private readonly object _dataSummaryLock = new object();
        private readonly object _dataTypesDistributionLock = new object();

        private readonly object _exportsDistributionLock = new object();
        private readonly object _periodLock = new object();
        private ExportVm _selectedExport;
        private NotifyTaskCompletion<KeyValue<DateTime, long>[]> _exportsDistribution;
        private NotifyTaskCompletion<DataSummaryItem[]> _dataSummary;
        private NotifyTaskCompletion<KeyValue<string, long>[]> _dataTypesDistribution;
        private NotifyTaskCompletion<string> _period;
        private NotifyTaskCompletion<ExportVm[]> _allExport;

        public ExportGroupVm(WindsorContainer applicationWindsorContainer, ExportGroup model) : base(applicationWindsorContainer, model)
        {
            this.ExportGroup = model;
            this.Exports = new ViewModelVirtualizingIoCObservableCollection<ExportVm, SnooperExportBase>(this.ExportGroup.Exports, applicationWindsorContainer);
            this.ExportGroups = new ViewModelVirtualizingIoCObservableCollection<ExportGroupVm, ExportGroup>(this.ExportGroup.ExportGroups, applicationWindsorContainer);

            this.Exports.CollectionChanged += this.ExportsOnCollectionChanged;
            this.ExportGroups.CollectionChanged += this.ExportGroupsOnCollectionChanged;

            model.PropertyChanged += this.Data_PropertyChanged;

            this.Exports.CollectionChanged += this.ExportResults_CollectionChanged;
            this.ExportGroups.CollectionChanged += this.ExportGroups_CollectionChanged;
        }

        public ViewModelVirtualizingIoCObservableCollection<ExportVm, SnooperExportBase> Exports { get;  }

        public ViewModelVirtualizingIoCObservableCollection<ExportGroupVm, ExportGroup> ExportGroups { get; }
        public ExportGroup ExportGroup { get;}

        public string Name
        {
            get => this.ExportGroup.Name;
            set => this.ExportGroup.Name = value;
        }

        public ExportVm SelectedExport
        {
            get => this._selectedExport;
            set
            {
                this._selectedExport = value;
                this.OnPropertyChanged();

                Task.Factory.StartNew(() =>
                {
                    if(this._selectedExport != null)
                    {
                        ExportResultMessage.SendExportResultMessage(this._selectedExport, ExportResultMessage.MessageType.ExportResultSelected);
                    }
                });
            }
        }

        [IgnoreAutoChangeNotification]
        public int ExportsCount => this.AllExport.Result.Count();

        [IgnoreAutoChangeNotification]
        public NotifyTaskCompletion<ExportVm[]> AllExport
        {
            get
            {
                lock (this.Exports.SyncRoot)
                {
                    return this._allExport
                           ?? (this._allExport =
                               new NotifyTaskCompletion<ExportVm[]>(()=>Task.Run(() => this.RecursiveExportResults(this).ToArray()), () => this.OnPropertyChanged(nameof(this.AllExport)), new ExportVm[]{}, false));
                }
            }
        }

        [IgnoreAutoChangeNotification]
        public ConcurrentObservableCollection<CallVm> Calls { get; } = new ConcurrentObservableCollection<CallVm>();

        [IgnoreAutoChangeNotification]
        public ConcurrentObservableCollection<ChatMessageVm> ChatMessages { get; } = new ConcurrentObservableCollection<ChatMessageVm>();

        [IgnoreAutoChangeNotification]
        public ConcurrentObservableCollection<EmailVm> Emails { get; } = new ConcurrentObservableCollection<EmailVm>();
        public bool IsGroup => true;

        [IgnoreAutoChangeNotification]
        public NotifyTaskCompletion<string> Period
        {
            get
            {
                lock (this._periodLock)
                {
                    return this._period
                           ?? (this._period =
                               new NotifyTaskCompletion<string>(() => Task.Run(() => this.GetPeriod()), () => this.OnPropertyChanged(nameof(this.Period)), false));
                }
            }
        }

        private string GetPeriod()
        {
            ExportVm[] results = this.AllExport;

            var firstTimeStamp = DateTime.MinValue;
            var lastTimeStamp = DateTime.MaxValue;

            var first = results.FirstOrDefault();

            if(first != null) { firstTimeStamp = first.Export.TimeStampFirst; }

            var last = results.LastOrDefault();

            if(last != null) { lastTimeStamp = first.Export.TimeStampLast; }

            var newPeriod = $"{(firstTimeStamp != DateTime.MinValue? firstTimeStamp.ToString() : "????")} - {(lastTimeStamp != DateTime.MaxValue? lastTimeStamp.ToString() : "????")}";

            return newPeriod;
        }

        [IgnoreAutoChangeNotification]
        public NotifyTaskCompletion<KeyValue<string, long>[]> DataTypesDistribution
        {
            get
            {
                lock (this._dataTypesDistributionLock)
                {
                    return this._dataTypesDistribution
                           ?? (this._dataTypesDistribution =
                               new NotifyTaskCompletion<KeyValue<string, long>[]>(() => Task.Run(() => this.GetDataTypesDistribution()), () => this.OnPropertyChanged(nameof(this.DataTypesDistribution)), false));
                }
            }
        }

        private KeyValue<string, long>[] GetDataTypesDistribution()
        {
            ExportVm[] recursiveResults = this.AllExport;

            // get all data
            var data = from er in recursiveResults
                group er by er.ExporterType
                into cg
                select new KeyValue<string, long>(cg.Key, cg.Count());


            var sum = data.Sum(x => x.Value);
            var result = (from d in data
                group d by (double) d.Value / sum > 0.05? d.Key : "Other"
                into dg
                select new KeyValue<string, long>(String.IsNullOrWhiteSpace(dg.Key)? "(unknown)" : dg.Key, dg.Sum(x => x.Value))).ToArray();


            return result;
        }

        [IgnoreAutoChangeNotification]
        public NotifyTaskCompletion<DataSummaryItem[]> DataSummary
        {
            get
            {
                lock (this._dataSummaryLock)
                {
                    return this._dataSummary
                           ?? (this._dataSummary =
                               new NotifyTaskCompletion<DataSummaryItem[]>(() => Task.Run(() => this.GetDataSummary()), () => this.OnPropertyChanged(nameof(this.DataSummary)), false));
                }
            }
        }

        private DataSummaryItem[] GetDataSummary()
        {
            ExportVm[] recursiveResults = this.AllExport;

            // get all data
            var data = from er in recursiveResults
                group er by er.ExporterType
                into cg
                select new KeyValue<string, long>(cg.Key, cg.Count());

            //  var enumerated = data.ToList();

            //  return data;

            var sum = data.Sum(x => x.Value);
            // show only those who has more than 5% and also up to top 5 protocols:
            var result = (from d in data
                orderby d.Value descending
                select new DataSummaryItem
                {
                    Name = String.IsNullOrWhiteSpace(d.Key)? "(unknown)" : d.Key,
                    TotalCount = d.Value,
                    Percent = (float) d.Value / sum * 100
                }).ToArray();

            return result;
        }

        [IgnoreAutoChangeNotification]
        public NotifyTaskCompletion<KeyValue<DateTime, long>[]> ExportsDistribution
        {
            get
            {
                lock (this._exportsDistributionLock)
                {
                    return this._exportsDistribution
                           ?? (this._exportsDistribution =
                               new NotifyTaskCompletion<KeyValue<DateTime, long>[]>(() => Task.Run(()=> this.GetExportsDistribution()), () => this.OnPropertyChanged(nameof(this.ExportsDistribution)), false));
                }
            }
        }
        private  KeyValue<DateTime, long>[] GetExportsDistribution()
        {
            return (from p in (ExportVm[])this.AllExport
                          group p by new DateTime(p.TimeStamp.Year, p.TimeStamp.Month, p.TimeStamp.Day, p.TimeStamp.Hour, p.TimeStamp.Minute, p.TimeStamp.Second, DateTimeKind.Utc)
                        into pg
                              orderby pg.Key
                              select new KeyValue<DateTime, long>(pg.Key, pg.Count(), pg.FirstOrDefault())).ToArray();
        }



        public NodeModel[] CommunicationNodes => null;

        [IgnoreAutoChangeNotification]
        public ICommand CAddNewGroup => new RelayCommand(this.AddNewGroup);

        [IgnoreAutoChangeNotification]
        public ICommand CDelete => new RelayCommand(this.Delete);

        public ExportVm SelectedExportResult { get; set; }

        [IgnoreAutoChangeNotification]
        public RelayCommand<ExportVm> CNavigateToExportDetail => new RelayCommand<ExportVm>(exportVm => this.NavigationService.Show(typeof(ExportDetailVm), exportVm));

        [IgnoreAutoChangeNotification]
        public RelayCommand<ExportVm> CSelectExport => new RelayCommand<ExportVm>(exportVm => this.SelectedExport = exportVm);

        public void AddNewGroup()
        {
            Debugger.Break();
            //this.ExportGroup.AddNewGroup("New group");
        }

        public void Delete() { this.ExportGroup.Delete(true); }

        public void MoveGroup(ExportGroupVm newParentGroup)
        {
            //if(newParentGroup == null || newParentGroup.ExportGroup.Id == this.ExportGroup.ParentId) { return; }

            //var database = this.ExportGroup.PersistenceProvider;
            //this.ExportGroup.PersistenceProvider.Delete(this.ExportGroup, true, true);
            //this.ExportGroup.ParentId = newParentGroup.ExportGroup.Id;
            //database.Insert(this.ExportGroup, true, true);
        }

        public void MoveGroupToParentId(string newParent)
        {
            //if(string.IsNullOrEmpty(newParent) || newParent == this.ExportGroup.ParentId) { return; }

            //var database = this.ExportGroup.PersistenceProvider;
            //this.ExportGroup.PersistenceProvider.Delete(this.ExportGroup, true, true);
            //this.ExportGroup.ParentId = newParent;
            //database.Insert(this.ExportGroup, true, true);
        }

        public void NewExportAdded(ExportVm exportVm) { }

        public IEnumerable<ExportVm> RecursiveExportResults(ExportGroupVm exportGroup)
        {
            foreach(var result in exportGroup.Exports)
            {
                yield return result;
            }

            foreach(var group in exportGroup.ExportGroups)
            {
                var subResults = this.RecursiveExportResults(group);

                foreach(var childResult in subResults)
                {
                    yield return childResult;
                }
            }
        }

        public void Updated()
        {
            this.AllExport.ReRun();
            this.Period.ReRun();
            this.DataTypesDistribution.ReRun();
            this.DataSummary.ReRun();
            this.ExportsDistribution.ReRun();

            this.OnPropertyChanged("CommunicationNodes");
            this.OnPropertyChanged("ExportsDistribution");
            this.OnPropertyChanged("DataSummary");
            this.OnPropertyChanged("DataTypesDistribution");
            this.OnPropertyChanged("Period");
            this.OnPropertyChanged("AllExport");
            this.OnPropertyChanged("ExportsCount");
            this.OnPropertyChanged("AllImages");
            this.OnPropertyChanged("AllMessages");
            this.OnPropertyChanged("AllCredentials");
            this.OnPropertyChanged("AllCalls");
            this.OnPropertyChanged("AllWebPages");

            this.OnPropertyChanged(nameof(this.Calls));
            this.OnPropertyChanged(nameof(this.Emails));
            this.OnPropertyChanged(nameof(this.Exports));
        }
        

        private void Data_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(this.Name)) { this.OnPropertyChanged(nameof(this.Name)); }
        }

        private void ExportGroups_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) { this.Updated(); }

        private void ExportGroupsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if(notifyCollectionChangedEventArgs.Action != NotifyCollectionChangedAction.Add) { return; }
            foreach(var exportGroupObj in notifyCollectionChangedEventArgs.NewItems)
            {
                var exportGroup = exportGroupObj as ExportGroupVm;
                if(exportGroup == null)
                {
                    continue;
                }
                exportGroup.Exports.CollectionChanged += this.ExportsOnCollectionChanged;
            }
        }

        private void ExportResults_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) { this.Updated(); }

        private void ExportsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            switch(notifyCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    this.NewExportAdded(notifyCollectionChangedEventArgs);
                    break;
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Reset:
                    this.Updated();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void NewExportAdded(NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            foreach(var exportObj in notifyCollectionChangedEventArgs.NewItems)
            {
                var export = exportObj as ExportVm;
                if(export == null)
                {
                    continue;
                }
                foreach(var call in export.Calls) { this.Calls.Add(call); }
                foreach(var email in export.Emails) { this.Emails.Add(email); }
                foreach(var chatMessage in export.ChatMessages) { this.ChatMessages.Add(chatMessage); }
            }
        }

        public class DataSummaryItem
        {
            public string Name { get; set; }
            public long TotalCount { get; set; }
            public float Percent { get; set; }
        }
        
    }
}