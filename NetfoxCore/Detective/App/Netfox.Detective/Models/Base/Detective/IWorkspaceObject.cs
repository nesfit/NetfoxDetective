using Netfox.Core.Models;

namespace Netfox.Detective.Models.Base.Detective
{
    public interface IWorkspaceObject
    {
        InvestigationInfo Workspace { get; set; }
    }
}