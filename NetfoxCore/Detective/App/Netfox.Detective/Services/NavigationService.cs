using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Netfox.Detective.Interfaces;
using Netfox.Detective.ViewModels;

namespace Netfox.Detective.Services
{
    public sealed class NavigationService : DetectiveApplicationServiceBase
    {
        public ICrossContainerHierarchyResolver CrossContainerHierarchyResolver { get; }
        private RelayCommand<Type> _cShowByType;
        private RelayCommand<ViewModelBase> _cShowByVm;

        public NavigationService(ICrossContainerHierarchyResolver crossContainerHierarchyResolver)
        {
            this.CrossContainerHierarchyResolver = crossContainerHierarchyResolver;
        }

        public delegate void NavigationAction(NavigationServiceArgs arguments);

        public event NavigationAction NavigateTo;

        public void Show(Type typeofvm, bool focus = true)
        {
            var handler = this.NavigateTo;
            var viewModel = this.CrossContainerHierarchyResolver.Resolve(typeofvm, null);
            handler?.Invoke(new NavigationServiceArgs(viewModel, focus));
        }

        public void Show(Type typeofvm, object model, bool focus = true)
        {
            var handler = this.NavigateTo;
            var viewModel = this.CrossContainerHierarchyResolver.Resolve(typeofvm, model);
            handler?.Invoke(new NavigationServiceArgs(viewModel, focus));
        }

        public RelayCommand<Type> CShowByType =>
            this._cShowByType ?? (this._cShowByType = new RelayCommand<Type>((t) => this.Show(t)));

        public RelayCommand<ViewModelBase> CShowByVm => this._cShowByVm ??
                                                        (this._cShowByVm =
                                                            new RelayCommand<ViewModelBase>((t) =>
                                                                this.Show(t.GetType())));

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