using Netfox.Framework.Models.PmLib.Frames;

namespace Netfox.Detective.ViewModelsDataEntity.Conversations
{
    public class QualityFrameVm
    {
        public enum FrameType
        {
            Normal,
            Malformed,
            Virtual
        }

        public PmFrameBase Frame { get; set; }
        public long Length { get; set; }
        public FrameType Type { get; set; }
    }
}