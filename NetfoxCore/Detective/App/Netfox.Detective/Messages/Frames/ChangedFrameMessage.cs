namespace Netfox.Detective.Messages.Frames
{
     class ChangedFrameMessage
    {
        public object Frame { get; set; }
        public uint FrameId { get; set; }
        public string ExportResultId { get; set; }
        public string CaptureId { get; set; }
        public uint ConversationIndex { get; set; }
        public bool BringToFront { get; set; } 
    }
}