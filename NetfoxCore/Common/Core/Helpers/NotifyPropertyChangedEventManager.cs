using System.ComponentModel;
using System.Windows;

namespace Netfox.Core.Helpers
{
    public class NotifyPropertyChangedEventManager: WeakEventManager
    {
        public static NotifyPropertyChangedEventManager CurrentManager
        {
            get
            {
                var manager_type = typeof(NotifyPropertyChangedEventManager);
                var manager = GetCurrentManager(manager_type) as NotifyPropertyChangedEventManager;
                if (manager == null)
                {
                    manager = new NotifyPropertyChangedEventManager();
                    SetCurrentManager(manager_type, manager);
                }
                return manager;
            }
        }

        public static void AddListener(INotifyPropertyChanged source, IWeakEventListener listener)
        {
            CurrentManager.ProtectedAddListener(source, listener);
            return;
        }

        public static void RemoveListener(INotifyPropertyChanged source, IWeakEventListener listener)
        {
            CurrentManager.ProtectedRemoveListener(source, listener);
            return;
        }
        protected override void StartListening(object source)
        {
            ((INotifyPropertyChanged)source).PropertyChanged += this.Source_PropertyChanged; return;
        }
        protected override void StopListening(object source)
        {
            ((INotifyPropertyChanged)source).PropertyChanged -= this.Source_PropertyChanged;
            return;
        }

        void Source_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            { CurrentManager.DeliverEvent(sender, e); };
        }
    }
}