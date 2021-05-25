using Castle.Windsor;

namespace Netfox.Core.Interfaces.ViewModels
{
    public interface IWindsorContainerChanger
    {
        IWindsorContainer InvestigationWindsorContainer { get; set; }
    }
}