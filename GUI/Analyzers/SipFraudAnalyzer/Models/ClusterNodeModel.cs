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

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Netfox.Core.Properties;
using Netfox.Detective.Properties;

namespace Netfox.AnalyzerSIPFraud.Models
{
    public class ClusterNodeModel : INotifyPropertyChanged
    {
        public string Label { get; set; }

        private string _toolTip = "";
        private bool _isExpanded = false;
        private bool _isLiefNode = true;

        public string ToolTip
        {
            get { return this._toolTip; }
            set
            {
                if (value == this._toolTip) { return; }
                this._toolTip = value;
                this.OnPropertyChanged();
            }
        }

        public ObservableCollection<ConnectionModel> Connections { get; } = new ObservableCollection<ConnectionModel>();

        public bool IsExpanded
        {
            get { return this._isExpanded; }
            set
            {
                if (value == this._isExpanded) { return; }
                this._isExpanded = value;
                this.OnPropertyChanged();
            }
        }

        public bool IsLiefNode
        {
            get { return this._isLiefNode; }
            set
            {
                if (value == this._isLiefNode) { return; }
                this._isLiefNode = value;
                this.OnPropertyChanged();
            }
        }

        public ClusterNodeModel ParrentCluster { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    }
}
