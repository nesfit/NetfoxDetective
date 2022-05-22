using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Castle.Core;
using Castle.Core.Logging;
using Castle.Windsor;
using GalaSoft.MvvmLight;
using Netfox.Core.Interfaces;
using Netfox.Detective.Services;
using Netfox.Detective.ViewModels.BgTasks;
using Netfox.Detective.ViewModels.Interfaces;
using Netfox.Detective.Views.Services;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModels
{
    public abstract class DetectiveViewModelBase : ViewModelBase, IDisposable, ISystemComponent, INotifyPropertyChanged,
        ILoggable
    {
        private ApplicationShell _applicationShell;
        private BgTasksManagerService _bgTasksManagerService;
        private BgTasksManagerVm _bgTasksManagerVm;
        protected bool _initialized;
        private bool _isActive;
        private bool _isHidden;
        private bool _isLoading;
        private bool _isSelected;
        private bool _isShown;
        private NavigationService _navigationService;

        public ILogger Logger { get; set; }

        protected DetectiveViewModelBase(IWindsorContainer applicationOrInverstiationWindsorContainer)
        {
            this.ApplicationOrInvestigationWindsorContainer = applicationOrInverstiationWindsorContainer;
        }

        [IgnoreAutoChangeNotification]
        public bool IsInDesign => LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        [IgnoreAutoChangeNotification] public ISystemServices SystemServices => new SystemServices();

        [IgnoreAutoChangeNotification]
        public NavigationService NavigationService => this._navigationService ?? (this._navigationService =
            this.ApplicationOrInvestigationWindsorContainer.Resolve<NavigationService>());

        [IgnoreAutoChangeNotification]
        public BgTasksManagerService BgTasksManagerService
            => this._bgTasksManagerService ?? (this._bgTasksManagerService =
                this.ApplicationOrInvestigationWindsorContainer.Resolve<BgTasksManagerService>());

        [IgnoreAutoChangeNotification]
        public BgTasksManagerVm BgTasksManagerVm => this._bgTasksManagerVm ?? (this._bgTasksManagerVm =
            this.ApplicationOrInvestigationWindsorContainer.Resolve<BgTasksManagerVm>());

        [IgnoreAutoChangeNotification]
        public IWindsorContainer ApplicationOrInvestigationWindsorContainer { get; protected set; }

        [IgnoreAutoChangeNotification]
        public ApplicationShell ApplicationShell => this._applicationShell ?? (this._applicationShell =
            this.ApplicationOrInvestigationWindsorContainer.Resolve<ApplicationShell>());

        public virtual string HeaderText { get; set; } = "Default header";

        /// <summary>
        ///     If true, VM can do operations as designed.
        ///     If false, VM should not compute anything because user do not care about results
        ///     This property should be checked before any resource consumption operation.
        /// </summary>
        public virtual bool IsActive
        {
            get { return this._isActive; }
            set
            {
                this._isActive = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Declares if the VIEW using this VM is hidden, not even tab header is shown
        /// </summary>
        public virtual bool IsHidden
        {
            get { return this._isHidden; }
            set
            {
                this._isHidden = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Declares if the corresponding VIEW is currently visible/shown on any PaneGroup in application
        ///     Many VMs might be set true on property IsVisible, but only one assigned to a PaneGroup can
        ///     have property IsShow set to true.
        ///     This property should not be set manually, only by navigation service.
        /// </summary>
        public virtual bool IsSelected
        {
            get { return this._isSelected; }
            set
            {
                this._isSelected = value;
                this.OnPropertyChanged();
                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Declares if the corresponding VIEW is currently visible/shown on any PaneGroup in application
        ///     Many VMs might be set true on property IsVisible, but only one assigned to a PaneGroup can
        ///     have property IsShow set to true.
        ///     This property should not be set manually, only in navigation service.
        /// </summary>
        public bool IsShown
        {
            get { return this._isShown; }
            set
            {
                this._isShown = value;
                this.OnPropertyChanged();
            }
        }

        public bool Initialized
        {
            get { return this._initialized; }
            set
            {
                this._initialized = value;
                this.OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get { return this._isLoading; }
            set
            {
                this._isLoading = value;
                this.OnPropertyChanged();
            }
        }

        [DoNotWire] public object View { get; set; }

        #region IDisposable

        public virtual void Dispose()
        {
        }

        #endregion

        public string ComponentName => this.GetType().Name;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            try
            {
                var handler = this.PropertyChangedHandler;
                handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            catch (Exception ex)
            {
                this.Logger?.Error($"Base VM exception during property change notification", ex);
                Debugger.Break();
            }
        }

        protected void OnPropertyChanged<T>(Expression<Func<T>> property)
        {
            var me = property.Body as MemberExpression;
            if (me == null || me.Expression != property.Parameters[0] || me.Member.MemberType != MemberTypes.Property)
            {
                throw new InvalidOperationException("Now tell me about the property");
            }

            var handler = this.PropertyChangedHandler;
            handler?.Invoke(this, new PropertyChangedEventArgs(me.Member.Name));
        }
    }
}