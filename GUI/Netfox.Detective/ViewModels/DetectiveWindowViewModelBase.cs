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

using System;
using System.ComponentModel;
using Castle.Windsor;
using Netfox.Core.Interfaces;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModels
{
    [NotifyPropertyChanged]
    public abstract class DetectiveWindowViewModelBase : DetectiveViewModelBase, INotifyPropertyChanged, IDisposable, ISystemComponent
    {
        private bool _isActive;
        private bool _isHidden;

        protected DetectiveWindowViewModelBase(IWindsorContainer applicationWindsorContainer) : base(applicationWindsorContainer) { }
        public abstract override string HeaderText { get; }

        public override bool IsActive
        {
            get => this._isActive;
            set
            {
                this._isActive = value;
                base.IsActive = value;
                this.OnPropertyChanged();
                this.RaisePropertyChanged();
            }
        }

        public override bool IsHidden
        {
            get => this._isHidden;
            set
            {
                this._isHidden = value;
                base.IsHidden = value;
                if(!value) { this.Show(); }
                this.OnPropertyChanged();
                this.RaisePropertyChanged();
            }
        }

        public Type ViewType { get; set; }
        public new WeakReference View { get; set; }

        public virtual void Close()
        {
            dynamic view = this.View.IsAlive? this.View.Target : null;
            if(view != null) { view.Close(); }
        }

        public void Hide() { this.IsHidden = true; }

        public void Show()
        {
            if(this.IsHidden) { this.IsHidden = false; }
            if(this.View?.Target == null) { this.View = new WeakReference(this.ApplicationOrInvestigationWindsorContainer.Resolve(this.ViewType)); }
            dynamic view = (this.View.IsAlive)? this.View.Target : new WeakReference(this.ApplicationOrInvestigationWindsorContainer.Resolve(this.ViewType));
            view.DataContext = this;
            view.Show();
        }
    }
}