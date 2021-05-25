using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Netfox.Detective.ViewModels.CommunicationNetwork
{
    public class ConnectionModel : INotifyPropertyChanged
    {
        private NodeModel _target;

        public NodeModel Target
        {
            get { return this._target; }
            set
            {
                if (value != this._target)
                {
                    this._target = value;
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