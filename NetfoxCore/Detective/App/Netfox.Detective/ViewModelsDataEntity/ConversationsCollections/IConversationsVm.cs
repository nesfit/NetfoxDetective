using System.Collections;
using System.ComponentModel;
using GalaSoft.MvvmLight.Command;
using Netfox.Core.Collections;
using Netfox.Core.Helpers;
using Netfox.Detective.ViewModels.Frame;
using Netfox.Detective.ViewModelsDataEntity.Conversations;
using Netfox.Detective.ViewModelsDataEntity.Exports;
using Netfox.Framework.Models;
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.Framework.Models.Snoopers;

namespace Netfox.Detective.ViewModelsDataEntity.ConversationsCollections
{
    public interface IConversationsVm : INotifyPropertyChanged
    {
        ViewModelVirtualizingIoCObservableCollection<FrameVm, PmFrameBase> Frames { get; }
        ViewModelVirtualizingIoCObservableCollection<ConversationVm, L3Conversation> L3Conversations { get; }
        ViewModelVirtualizingIoCObservableCollection<ConversationVm, L4Conversation> L4Conversations { get; }
        ViewModelVirtualizingIoCObservableCollection<ConversationVm, L7Conversation> L7Conversations { get; }
        ViewModelVirtualizingIoCObservableCollection<SnooperVm, ISnooper> UsedSnoopers { get; }

        ConversationVm CurrentConversation { get; set; }

        //ConversationVm[] ConversationsByTraffic { get; }
        //ConversationVm[] ConversationsByDuration { get; }
        ExportVm[] AllExportResults { get; }
        RelayCommand CAddConvGroupToExportClick { get; }
        ConversationsStatisticsVm ConversationsStatisticsVm { get; }
        bool IsSelected { get; set; }
        RelayCommand<ConversationVm> CShowConversationDetail { get; }
        RelayCommand<IList> CCreateConversationsGroup { get; }
        RelayCommand<FrameVm> CShowFrameDetail { get; }
        NotifyTaskCompletion<ConversationVm[]> ConversationsByTraffic { get; }
        NotifyTaskCompletion<ConversationVm[]> ConversationsByDuration { get; }
        ConversationVm SetCurrentConversationByFrame(FrameVm frameVm);
    }
}