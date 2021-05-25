using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using Castle.Windsor;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using log4net.Core;
using Netfox.Core.Interfaces.Views;
using Netfox.Detective.Core.BaseTypes;
using Netfox.Logger;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModels.Outputs
{
    public sealed class MainOutputVm : DetectiveApplicationPaneViewModelBase
    {
        public MainOutputVm(WindsorContainer applicationWindsorContainer, NetfoxOutputAppender netfoxOutputAppender) :
            base(applicationWindsorContainer)
        {
            this.NetfoxOutputAppender = netfoxOutputAppender;
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
                this.View = this.ApplicationOrInvestigationWindsorContainer.Resolve<IMainOutputView>());
            this.IsHidden = false;
            this.IsSelected = true;
            this.DockPositionPosition = DetectiveDockPosition.DockedBottom;
            this.NetfoxOutputAppender.OutputMessages.CollectionChanged += this.OutputMessagesOnCollectionChanged;
        }

        public override string HeaderText => "Main output";
        public NetfoxOutputAppender NetfoxOutputAppender { get; }

        public LoggingEvent SelectedLogMessage { get; set; }

        [IgnoreAutoChangeNotification]
        public RelayCommand CCopyItemToClipBoard
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var selectedLogMessage = this.SelectedLogMessage;
                    if (selectedLogMessage != null)
                    {
                        Clipboard.SetData(DataFormats.Text, selectedLogMessage.ToString());
                    }
                });
            }
        }

        [SafeForDependencyAnalysis]
        public ObservableCollection<LoggingEvent> OutputMessages => this.NetfoxOutputAppender.OutputMessages;

        private void OutputMessagesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args?.NewItems == null || args.NewItems.Count == 0)
            {
                return;
            }

            var sysmsg = args.NewItems[0] as LoggingEvent;
            this.SelectedLogMessage = sysmsg;
        }
    }
}