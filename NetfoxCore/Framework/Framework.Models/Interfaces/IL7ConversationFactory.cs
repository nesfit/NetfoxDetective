using Netfox.Core.Enums;

namespace Netfox.Framework.Models.Interfaces
{
    public interface IL7ConversationFactory
    {
        L7Conversation Create(FsUnidirectionalFlow upFlow, FsUnidirectionalFlow downFlow);
        L7Conversation Create(FsUnidirectionalFlow flow, DaRFlowDirection flowDirection);
    }
}