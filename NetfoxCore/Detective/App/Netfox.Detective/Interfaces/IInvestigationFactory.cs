using System.Threading.Tasks;
using Netfox.Core.Interfaces;
using Netfox.Detective.Interfaces.Models.Base;

namespace Netfox.Detective.Interfaces
{
    public interface IInvestigationFactory
    {
        IInvestigation CreateInternal(IInvestigationInfo investigationInfo);
        Task<IInvestigation> Create(IInvestigationInfo investigationInfo);
    }
}