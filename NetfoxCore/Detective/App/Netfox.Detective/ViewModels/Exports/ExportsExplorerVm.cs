using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Castle.Core;
using Netfox.Detective.Interfaces;
using Netfox.Detective.Messages;
using Netfox.Detective.Messages.Exports;
using Netfox.Detective.Messages.Investigations;
using Netfox.Detective.ViewModelsDataEntity.Exports;
using Netfox.Detective.ViewModelsDataEntity.Investigations;

namespace Netfox.Detective.ViewModels.Exports
{
    public class ExportsExplorerVm : INotifyPropertyChanged
    {
        private InvestigationVm _currentInvestigation;
        private ObservableCollection<ExplorerItem> _currentItems;
        private ObservableCollection<ExplorerItem> _currentPath;
        private readonly IDetectiveMessenger _messenger;

        public ExportsExplorerVm()
        {
            this._messenger = new DetectiveMvvmLightMessenger();
            Task.Factory.StartNew(() =>
            {
                this._messenger.Register<ChangedExportGroupMessage>(this, this.ExportGroupActionHandler);

                this._messenger.Register<OpenedInvestigationMessage>(this, this.InvestigationActionHandler);
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
            if (this._currentInvestigation == null)
            {
                return;
            }

            if (item == null || item.Type == ExplorerItem.ItemType.Investigation)
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
                if (item.Type != ExplorerItem.ItemType.Investigation)
                {
                    if (this.CurrentPath.Contains(item))
                    {
                        var newPath = new ObservableCollection<ExplorerItem>();
                        foreach (var explorerItem in this.CurrentPath)
                        {
                            newPath.Add(explorerItem);
                            if (explorerItem == item)
                            {
                                break;
                            }
                        }

                        this.CurrentPath = newPath;
                    }
                    else
                    {
                        this.CurrentPath.Add(item);
                    }


                    this.CurrentItems = new ObservableCollection<ExplorerItem>();
                }

                switch (item.Type)
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
                        this._messenger.AsyncSend(new SelectedExportResultMessage
                        {
                            ExportVm = item.Result
                        });

                        break;
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
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
            foreach (var exportsGroup in exportGroups)
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
            foreach (var result in exportResults)
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

        private void ExportGroupActionHandler(ChangedExportGroupMessage message)
        {
            var ceg = message.ExportGroupVm as ExportGroupVm;
        }

        private void InvestigationActionHandler(OpenedInvestigationMessage message)
        {
            var cinv = message.InvestigationVm as InvestigationVm;
            if (cinv != null)
            {
                this.CurrentInvestigation = cinv;
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