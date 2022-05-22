namespace Netfox.Detective.Messages.Captures
{
     class ChangedCaptureMessage
    {
        public object CaptureVm { get; set; }
        public string CaptureId { get; set; }
        public bool BringToFront { get; set; }
    }
}