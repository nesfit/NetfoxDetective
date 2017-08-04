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

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Netfox.Core.Collections;

namespace Netfox.Detective.ViewModels.CommunicationNetwork
{
    public class NodeModel : INotifyPropertyChanged
    {
        //private string _toolTip;

        //public string ToolTip
        //{
        //    get { return _toolTip; }
        //    set
        //    {
        //        if (value != _toolTip)
        //        {
        //            _toolTip = value; OnPropertyChanged();
        //        }
        //    }
        //}

        private ConcurrentObservableCollection<ConnectionModel> _connections = new ConcurrentObservableCollection<ConnectionModel>();
        private string _label;

        public string Label
        {
            get { return this._label; }
            set
            {
                if(value != this._label)
                {
                    this._label = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public ConcurrentObservableCollection<ConnectionModel> Connections
        {
            get { return this._connections; }
            set
            {
                if(value != this._connections)
                {
                    this._connections = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = this.PropertyChanged;
            if(handler != null) { handler(this, new PropertyChangedEventArgs(propertyName)); }
        }
    }
}