// Copyright (c) 2017 Jan Pluskal
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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Castle.Core.Internal;
using Castle.Core.Logging;
using GalaSoft.MvvmLight.Threading;
using Netfox.AnalyzerAppIdent.Properties;
using Netfox.AppIdent.EPI;
using Netfox.Core.Interfaces;
using Telerik.Windows.Controls.Charting;
using Telerik.Windows.Data;

namespace Netfox.AnalyzerAppIdent.ViewModels
{
    public class ProtocolModelsVm : INotifyPropertyChanged, ILoggable
    {
        public AppIdentMainVm AppIdentMainVm { get; set; }
        private EPIProtocolModel _selectedAppProtocolModel;
        private QueryableCollectionView _selectedAppProtocolModelFeatureVectorValues;

        public ProtocolModelsVm(AppIdentMainVm appIdentMainVm)
        {
            this.AppIdentMainVm = appIdentMainVm;
            this.SelectedAppProtocolModel = this.AppIdentMainVm.AppProtoModelEval?.ProtocolModels?.FirstOrDefault();
            this.SelectedAppProtocolModels.CollectionChanged += (sender, args) =>
            {
                try { this.SelectedApplicationProtocolModelChanged(args); }
                catch(Exception e) {
                    Debugger.Break();
                    Console.WriteLine(e);
                }
            };

            DispatcherHelper.UIDispatcher.Invoke(() => { this.SelectedAppProtocolModelsSeriesMappings = new SeriesMappingCollection(); });

            this.SelectedAppProtocolModelsFeatureVectorValues.CollectionChanged += (sender, args) =>
             {
                 this.SelectedAppProtocolModelsSeriesMappings.Clear();
                 if(args.Action != NotifyCollectionChangedAction.Add) return;

                //this.SelectedAppProtocolModelsSeriesMappings.SuspendNotifications();



                for (int i = 0; i < this.SelectedAppProtocolModelsFeatureVectorValues.Count; i++)
                 {
                     var mapping = new SeriesMapping
                     {
                         CollectionIndex = i,
                         LegendLabel = this.SelectedAppProtocolModelsLabels[i]
                     };
                     mapping.ItemMappings.Add(new ItemMapping("Name", DataPointMember.XCategory));
                     mapping.ItemMappings.Add(new ItemMapping("PositiveSigma", DataPointMember.Open));
                     mapping.ItemMappings.Add(new ItemMapping("Max", DataPointMember.High));
                     mapping.ItemMappings.Add(new ItemMapping("Min", DataPointMember.Low));
                     mapping.ItemMappings.Add(new ItemMapping("NegativeSigma", DataPointMember.Close));
                     mapping.SeriesDefinition = new CandleStickSeriesDefinition();

                     this.SelectedAppProtocolModelsSeriesMappings.Add(mapping);
                 }

                this.OnPropertyChanged(nameof(this.SelectedAppProtocolModelsSeriesMappings));

                //< !--< telerik:SeriesMapping LegendLabel = "{Binding SelectedAppProtocolModelsLabels[0]}" CollectionIndex = "0" >

                //            < telerik:SeriesMapping.SeriesDefinition >

                //                 < telerik:CandleStickSeriesDefinition />

                //              </ telerik:SeriesMapping.SeriesDefinition >

                //               < telerik:SeriesMapping.ItemMappings >

                //                    < telerik:ItemMapping DataPointMember = "Open"
                //                            FieldName = "PositiveSigma" />
                //            < telerik:ItemMapping DataPointMember = "High"
                //                            FieldName = "Max" />
                //            < telerik:ItemMapping DataPointMember = "Low"
                //                            FieldName = "Min" />
                //            < telerik:ItemMapping DataPointMember = "Close"
                //                            FieldName = "NegativeSigma" />
                //            < telerik:ItemMapping DataPointMember = "XCategory"
                //                            FieldName = "Name" />
                //        </ telerik:SeriesMapping.ItemMappings >

                //this.SelectedAppProtocolModelsSeriesMappings.ResumeNotifications();
            };
        }

        private void SelectedApplicationProtocolModelChanged(NotifyCollectionChangedEventArgs args)
        {
            EPIProtocolModel item = null;
            if(args.NewItems != null && args.NewItems.Count > 0) item = args.NewItems[0] as EPIProtocolModel;
            switch(args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if(item == null) return;
                    var protoName = item.ApplicationProtocolName;
                    var featureVectorValues = item.FeatureVectorValues?.Values;
                    if(protoName.IsNullOrEmpty() || featureVectorValues == null)
                    {
                        Debugger.Break();
                        this.Logger?.Error("Selected protocol model features were null.");
                        return;
                    }
                    this.SelectedAppProtocolModelsLabels.Add(protoName);
                    this.SelectedAppProtocolModelsFeatureVectorValues.Add(new QueryableCollectionView(featureVectorValues));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (item == null) return;
                    var itemTobeRemoved = this.SelectedAppProtocolModelsLabels.FirstOrDefault(i => i == item.ApplicationProtocolName);
                    var index = this.SelectedAppProtocolModelsLabels.IndexOf(itemTobeRemoved);
                    this.SelectedAppProtocolModelsLabels.RemoveAt(index);
                    this.SelectedAppProtocolModelFeatureVectorValues.RemoveAt(index);
                    break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Reset:
                default:
                    this.SelectedAppProtocolModelsFeatureVectorValues.Clear();
                    this.SelectedAppProtocolModelsLabels.Clear();
                    break;
            }
            //this.OnPropertyChanged(nameof(this.SelectedAppProtocolModelsLabels));
        }

        public QueryableCollectionView SelectedAppProtocolModelFeatureVectorValues
        {
            get => this._selectedAppProtocolModelFeatureVectorValues;
            set
            {
                this._selectedAppProtocolModelFeatureVectorValues = value;
                this.OnPropertyChanged();
            }
        }

        public EPIProtocolModel SelectedAppProtocolModel
        {
            get => this._selectedAppProtocolModel;
            set
            {
                this._selectedAppProtocolModel = value;
                this.OnPropertyChanged();
                if(value != null) this.SelectedAppProtocolModelFeatureVectorValues = new QueryableCollectionView(value?.FeatureVectorValues?.Values);
            }
        }

        public ObservableCollection<EPIProtocolModel> SelectedAppProtocolModels { get; set; } = new ObservableCollection<EPIProtocolModel>();
        public ObservableCollection<QueryableCollectionView> SelectedAppProtocolModelsFeatureVectorValues { get; set; } = new ObservableCollection<QueryableCollectionView>();
        public ObservableCollection<string> SelectedAppProtocolModelsLabels { get; set; } = new ObservableCollection<string>();

        public SeriesMappingCollection SelectedAppProtocolModelsSeriesMappings { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

        #region Implementation of ILoggable
        public ILogger Logger { get; set; }
        #endregion
    }
}