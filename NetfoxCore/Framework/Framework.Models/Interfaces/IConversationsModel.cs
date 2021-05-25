using AlphaChiTech.VirtualizingCollection.Interfaces;
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.Framework.Models.Snoopers;

namespace Netfox.Framework.Models.Interfaces
{
    public interface IConversationsModel
    {
        //ICollection<ILxConversation> Conversations { get; }
        IObservableCollection<PmFrameBase> Frames { get; }
        IObservableCollection<L3Conversation> L3Conversations { get; }
        IObservableCollection<L4Conversation> L4Conversations { get; }

        IObservableCollection<L7Conversation> L7Conversations { get; }

        //ConversationsSummaryStatistics Statistics { get; }
        //IObservableCollection<PmFrameBase> Frames { get; }
        IObservableCollection<SnooperExportBase> SnooperExports { get; }
        IObservableCollection<ISnooper> UsedSnoopers { get; }
    }
}