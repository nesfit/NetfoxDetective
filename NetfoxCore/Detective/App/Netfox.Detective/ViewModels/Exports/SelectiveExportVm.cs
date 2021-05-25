using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Windows.Input;
using Castle.Core;
using Castle.Windsor;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using Netfox.Core.Interfaces.Views;
using Netfox.Detective.Interfaces;
using Netfox.Detective.Messages.Investigations;
using Netfox.Detective.ViewModelsDataEntity.Investigations;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModels.Exports
{
    [NotifyPropertyChanged]
    public class SelectiveExportVm : DetectiveApplicationPaneViewModelBase
    {
        private IDetectiveMessenger _messenger;

        public SelectiveExportVm(WindsorContainer applicationWindsorContainer) : base(applicationWindsorContainer)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
                this.View = this.ApplicationOrInvestigationWindsorContainer.Resolve<ISelectiveExportView>());
            this._messenger = this.ApplicationOrInvestigationWindsorContainer.Resolve<IDetectiveMessenger>();
            Task.Factory.StartNew(() =>
                this._messenger.Register<OpenedInvestigationMessage>(this, this.OpenedInvestigationMessageReceived));
        }

        #region Overrides of DetectivePaneViewModelBase

        public override string HeaderText => "Selective export";

        #endregion

        [DoNotWire] public InvestigationVm InvestigationVm { get; set; }

        [IgnoreAutoChangeNotification] public RelayCommand CExport => null;

        //this._CExport  ?? (this._CExport = new RelayCommand(() => this.InvestigationVm.ExportExecute(), () => this.InvestigationVm != null && this.InvestigationVm.ToExportConversations.Count > 0));

        [IgnoreAutoChangeNotification]
        public ICommand CClearToExport
        {
            get
            {
                //return new RelayCommand(() => ToExportConversations.Clear(), () => ToExportConversations.Count > 0);
                return new RelayCommand(() => this.InvestigationVm?.ToExportConversations.Clear());
            }
        }

        private void OpenedInvestigationMessageReceived(OpenedInvestigationMessage msg)
        {
            this.InvestigationVm = msg.InvestigationVm as InvestigationVm;

            if (this.InvestigationVm == null)
            {
                return;
            }

            this.InvestigationVm.ToExportConversations.CollectionChanged +=
                this.ToExportConversationsOnCollectionChanged;
        }

        [IgnoreAutoChangeNotification]
        private void ToExportConversationsOnCollectionChanged(object sender,
            NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            DispatcherHelper.RunAsync(() => this.CExport.RaiseCanExecuteChanged());
        }
    }
}