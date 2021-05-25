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

        private ConcurrentObservableCollection<ConnectionModel> _connections =
            new ConcurrentObservableCollection<ConnectionModel>();

        private string _label;

        public string Label
        {
            get { return this._label; }
            set
            {
                if (value != this._label)
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
                if (value != this._connections)
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
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}