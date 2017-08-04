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

using System.ComponentModel;
using System.Windows;

namespace Netfox.Core.Helpers
{
    public class NotifyPropertyChangedEventManager : WeakEventManager
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
