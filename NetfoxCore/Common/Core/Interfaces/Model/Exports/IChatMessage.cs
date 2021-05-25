namespace Netfox.Core.Interfaces.Model.Exports
{
    public interface IChatMessage : IExportBase
    {
        string Message { get; }
        string Sender { get; }
        string Receiver { get; }
    }
}