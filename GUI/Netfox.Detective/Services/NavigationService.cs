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
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Netfox.Detective.Interfaces;
using Netfox.Detective.ViewModels;

namespace Netfox.Detective.Services
{
    public enum NavigationAction
    {
        Show,
        Hide
    }

    public sealed class NavigationService : DetectiveApplicationServiceBase
    {
        public ICrossContainerHierarchyResolver CrossContainerHierarchyResolver { get; }
        private RelayCommand<Type> _cShowByType;
        private RelayCommand<ViewModelBase> _cShowByVm;

        public NavigationService(ICrossContainerHierarchyResolver crossContainerHierarchyResolver) { this.CrossContainerHierarchyResolver = crossContainerHierarchyResolver; }

        public delegate void NavigationAction(NavigationServiceArgs arguments);
        
        public event NavigationAction NavigateTo;

        public void Show(Type typeofvm, bool focus = true)
        {
            var handler = this.NavigateTo;
            var viewModel = this.CrossContainerHierarchyResolver.Resolve(typeofvm,null);
            handler?.Invoke(new NavigationServiceArgs(viewModel, focus));
        }

        public void Show(Type typeofvm, object model, bool focus = true)
        {
            var handler = this.NavigateTo;
            var viewModel = this.CrossContainerHierarchyResolver.Resolve(typeofvm, model);
            handler?.Invoke(new NavigationServiceArgs(viewModel, focus));
        }

        public RelayCommand<Type> CShowByType => this._cShowByType ?? (this._cShowByType = new RelayCommand<Type>((t) => this.Show(t)));
        public RelayCommand<ViewModelBase> CShowByVm => this._cShowByVm ?? (this._cShowByVm = new RelayCommand<ViewModelBase>((t) => this.Show(t.GetType())));

        public sealed class NavigationServiceArgs
        {
            public NavigationServiceArgs(DetectiveViewModelBase viewModel, bool focus)
            {
                this.Focus = focus;
                this.ViewModel = viewModel;
            }
            
            public DetectiveViewModelBase ViewModel { get; private set; }
            public bool Focus { get; set; } = true;
        }
    }
}