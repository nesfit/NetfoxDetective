namespace Netfox.Core.Interfaces.Model.Exports
{
    public interface ICallStream : ICall
    {
        string WavFilePath { get; set; }
    }
}