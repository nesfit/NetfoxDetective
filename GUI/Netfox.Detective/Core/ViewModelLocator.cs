using System.Dynamic;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Netfox.GUI.Detective.Core
{
    public class ViewModelLocator : DynamicObject, IWindsorInstaller, IViewModelLocator
    {
        public static IViewModelLocator Instance = new ViewModelLocatorInternal();
        public void Initialize() { Instance.Initialize(); }

        public bool IsInDebug => Instance.IsInDebug;

        // If you try to get a value of a property  
        // not defined in the class, this method is called. 
        public override bool TryGetMember(GetMemberBinder binder, out object result) { return Instance.TryGetMember(binder, out result); }
        // If you try to set a value of a property that is 
        // not defined in the class, this method is called. 
        public override bool TrySetMember(SetMemberBinder binder, object value) { return Instance.TrySetMember(binder, value); }
        public void Install(IWindsorContainer container, IConfigurationStore store) { Instance.Install(container, store); }
    }
}