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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Castle.Core;
using GalaSoft.MvvmLight.Messaging;
using Netfox.Core.Messages.Base;
using Netfox.Core.Messages.Exports;
using Netfox.Core.Messages.Views;
using Netfox.Detective.ViewModelsDataEntity.Exports;
using Netfox.Detective.ViewModelsDataEntity.Investigations;

namespace Netfox.Detective.ViewModels.Exports
{
    public class ExportsExplorerVm : INotifyPropertyChanged
    {
        private InvestigationVm _currentInvestigation;
        private ObservableCollection<ExplorerItem> _currentItems;
        private ObservableCollection<ExplorerItem> _currentPath;

        public ExportsExplorerVm()
        {
            Task.Factory.StartNew(() =>
            {
                Messenger.Default.Register<ExportGroupMessage>(this, this.ExportGroupActionHandler);
                Messenger.Default.Register<InvestigationMessage>(this, this.InvestigationActionHandler);
            });
        }
        [DoNotWire]
        public InvestigationVm CurrentInvestigation
        {
            get { return this._currentInvestigation; }
            set
            {
                this._currentInvestigation = value;
                this.Navigate(null);
            }
        }

        public ObservableCollection<ExplorerItem> CurrentPath
        {
            get { return this._currentPath; }
            set
            {
                this._currentPath = value;
                this.OnPropertyChanged();
            }
        }

        public ObservableCollection<ExplorerItem> CurrentItems
        {
            get { return this._currentItems; }
            set
            {
                this._currentItems = value;
                this.OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Navigate(ExplorerItem item)
        {
            if(this._currentInvestigation == null) { return; }

            if(item == null || item.Type == ExplorerItem.ItemType.Investigation)
            {
                this.CurrentPath = new ObservableCollection<ExplorerItem>
                {
                    new ExplorerItem
                    {
                        Type = ExplorerItem.ItemType.Investigation,
                        Name = this._currentInvestigation.Investigation.InvestigationInfo.InvestigationName
                    }
                };
                this.CurrentItems = new ObservableCollection<ExplorerItem>();
                this.AddGroupsToCurrentItems(this._currentInvestigation.ExportGroups);
            }
            else
            {
                if(item.Type != ExplorerItem.ItemType.Investigation)
                {
                    if(this.CurrentPath.Contains(item))
                    {
                        var newPath = new ObservableCollection<ExplorerItem>();
                        foreach(var explorerItem in this.CurrentPath)
                        {
                            newPath.Add(explorerItem);
                            if(explorerItem == item) { break; }
                        }
                        this.CurrentPath = newPath;
                    }
                    else
                    {
                        this.CurrentPath.Add(item);
                    }


                    this.CurrentItems = new ObservableCollection<ExplorerItem>();
                }

                switch(item.Type)
                {
                    case ExplorerItem.ItemType.Group:

                        this.AddGroupsToCurrentItems(item.Group.ExportGroups);
                        //todo  this.AddResultsToCurrentItems(item.Group.ExportResults);

                        break;
                    case ExplorerItem.ItemType.Result:

                        //todo   this.AddExportedDataToCurrentItems(item.Result, item.ResultState.ExportData);

                        break;

                    case ExplorerItem.ItemType.Data:

                        //todo  item.Result.SelectDataByDataObject(item.Data, true);
                        ExportResultMessage.SendExportResultMessage(item.Result, ExportResultMessage.MessageType.ExportResultSelected);
                        BringToFrontMessage.SendBringToFrontMessage("ExportContentView");

                        break;
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = this.PropertyChanged;
            if(handler != null) { handler(this, new PropertyChangedEventArgs(propertyName)); }
        }

        //private void AddExportedDataToCurrentItems(ExportVm result, IEnumerable<ExportDataVm> datas)
        //{
        //    foreach(var data in datas)
        //    {
        //        this.CurrentItems.Add(new ExplorerItem
        //        {
        //            Type = ExplorerItem.ItemType.Data,
        //            Name = data.ExportedData.Description,
        //            Data = data,
        //            Result = ResultState,
        //            Period = data.ExportedData.NetworkTimeStamp.ToString()
        //        });
        //    }
        //}

        private void AddGroupsToCurrentItems(IEnumerable<ExportGroupVm> exportGroups)
        {
            foreach(var exportsGroup in exportGroups)
            {
                this.CurrentItems.Add(new ExplorerItem
                {
                    Type = ExplorerItem.ItemType.Group,
                    Name = exportsGroup.Name,
                    Group = exportsGroup,
                    Period = exportsGroup.Period
                });
            }
        }

        private void AddResultsToCurrentItems(IEnumerable<ExportVm> exportResults)
        {
            foreach(var result in exportResults)
            {
                this.CurrentItems.Add(new ExplorerItem
                {
                    Type = ExplorerItem.ItemType.Result,
                    Name = result.Name,
                    Result = result,
                    Period = result.Period,
                    ItemsCount = result.EventsCount
                });
            }
        }

        private void ExportGroupActionHandler(ExportGroupMessage message)
        {
            if(message.Type == ExportGroupMessage.MessageType.CurrentExportGroupChanged) { var ceg = message.ExportGroupVm as ExportGroupVm; }
        }

        private void InvestigationActionHandler(InvestigationMessage message)
        {
            if(message.Type == InvestigationMessage.MessageType.CurrentInvestigationChanged)
            {
                var cinv = message.InvestigationVm as InvestigationVm;
                if(cinv != null) { this.CurrentInvestigation = cinv; }
            }
        }

        public class ExplorerItem
        {
            public enum ItemType
            {
                Investigation,
                Group,
                Result,
                Data
            }

            public ItemType Type { get; set; }
            public string Period { get; set; }
            public int ItemsCount { get; set; }
            public string Name { get; set; }
            public ExportGroupVm Group { get; set; }
            public ExportVm Result { get; set; }
            //todo  public ExportDataVm Data { get; set; }
        }
    }
}