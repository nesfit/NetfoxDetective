using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using AlphaChiTech.Virtualization;
using AlphaChiTech.VirtualizingCollection.Interfaces;
using Castle.Windsor;
using Netfox.Core.Interfaces;
using Netfox.Core.Interfaces.ViewModels;
using Netfox.Detective.Models;
using Netfox.Detective.Models.Conversations;
using Netfox.Detective.Models.Exports;
using Netfox.Framework.Models;
using Netfox.Framework.Models.Interfaces;
using Netfox.Framework.Models.PmLib.Captures;
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.Framework.Models.Snoopers;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.Interfaces.Models.Base
{
    public interface IInvestigation : IConversationsModel, IWindsorContainerChanger, INotifyPropertyChanged, IInitializable
    {


        [NotMapped]
        [IgnoreAutoChangeNotification]
        IObservableCollection<PmCaptureBase> Captures { get; }


        [DataMember]
        IInvestigationInfo InvestigationInfo { get; }

        [NotMapped]
        ConcurrentIObservableVirtualizingObservableCollection<OperationLog> OperationLogs { get; set; }

        [IgnoreAutoChangeNotification]
        [NotMapped]
        IObservableCollection<ConversationsGroup> ConversationsGroups { get; }


        [IgnoreAutoChangeNotification]
        [NotMapped]
        IObservableCollection<ExportGroup> ExportsGroups { get; }

        [NotMapped]
        [IgnoreAutoChangeNotification]
        IObservableCollection<PmFrameBase> AllFrames { get; }

        [NotMapped]
        [IgnoreAutoChangeNotification]
        IObservableCollection<Detective.Models.SourceLog.SourceLog> SourceLogs { get; }

        [NotMapped]
        [IgnoreAutoChangeNotification]
        IObservableCollection<PmFrameBase> Frames { get; }

        [NotMapped]
        [IgnoreAutoChangeNotification]
        IObservableCollection<L3Conversation> L3Conversations { get; }

        [NotMapped]
        [IgnoreAutoChangeNotification]
        IObservableCollection<L4Conversation> L4Conversations { get; }

        [NotMapped]
        [IgnoreAutoChangeNotification]
        IObservableCollection<L7Conversation> L7Conversations { get; }

        [NotMapped]
        [IgnoreAutoChangeNotification]
        IObservableCollection<SnooperExportBase> SnooperExports { get; }

        [NotMapped]
        [IgnoreAutoChangeNotification]
        IObservableCollection<ISnooper> UsedSnoopers { get; }

        Task Initialize();

        event PropertyChangedEventHandler PropertyChanged;

        [NotMapped]
        IWindsorContainer InvestigationWindsorContainer { get; set; }
    }
}
