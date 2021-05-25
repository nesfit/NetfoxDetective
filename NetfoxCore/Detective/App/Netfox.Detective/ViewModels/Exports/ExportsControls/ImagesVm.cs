using System.ComponentModel;
using Castle.Windsor;

namespace Netfox.Detective.ViewModels.Exports.ExportsControls
{
    public class ImagesVm : DetectiveViewModelBase, INotifyPropertyChanged
    {
        public ImagesVm(WindsorContainer applicationWindsorContainer) : base(applicationWindsorContainer)
        {
        }
    }
}