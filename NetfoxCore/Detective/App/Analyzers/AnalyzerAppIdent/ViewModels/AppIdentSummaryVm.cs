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
using System.ComponentModel;
using System.Linq;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using Netfox.AppIdent.Statistics;
using Telerik.Windows.Data;

namespace Netfox.AnalyzerAppIdent.ViewModels
{
    public class AppIdentSummaryVm : INotifyPropertyChanged
    {
        public ApplicationProtocolClassificationStatisticsMeter PrecMeasure { get; set; }
        public AppIdentMainVm AppIdentMainVm { get; set; }
        private QueryableCollectionView _summaryStatistic;

        public QueryableCollectionView SummaryStatistic
        {
            get => this._summaryStatistic;
            set
            {
                this._summaryStatistic = value;
                this.OnPropertyChanged();
            }
        }

        public AppIdentSummaryVm(AppIdentMainVm appIdentMainVm)
        {
            this.AppIdentMainVm = appIdentMainVm;
            appIdentMainVm.EpiPrecMeasureObservable.Subscribe(this.UpdateSummaryStatistic);
        }

        public AppIdentSummaryVm(ApplicationProtocolClassificationStatisticsMeter precMeasure)
        {
            this.UpdateSummaryStatistic(precMeasure);
        }

        private void UpdateSummaryStatistic(ApplicationProtocolClassificationStatisticsMeter precMeasure)
        {
            if(precMeasure == null)
            {
                return;
            }
            this.PrecMeasure = precMeasure;
            this.SummaryStatistic = new QueryableCollectionView(precMeasure.AppStatistics.Select(s => s.Value));
        }


        public void ExportPrecMeasureToCsv()
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "CSV file (*.csv)|*.csv|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == true)
            {
                try
                {
                    this.PrecMeasure.SaveToCsv(saveFileDialog1.FileName);
                }
                catch(Exception ex)
                {
                    MessageBoxResult result = MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
               
            }
        }

        public RelayCommand ExportPrecMeasureToCsvCommand => new RelayCommand(this.ExportPrecMeasureToCsv);

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null) { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    }
}
