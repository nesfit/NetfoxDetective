using System.IO.Abstractions;

namespace Netfox.Core.Interfaces
{
    public interface IInvestigationInfoLoader
    {
        IInvestigationInfo Load(FileInfoBase fileInfo);
    }
}