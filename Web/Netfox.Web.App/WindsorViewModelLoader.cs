using System;
using Castle.Windsor;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ViewModel.Serialization;

namespace Netfox.Web.App
{
    public class WindsorViewModelLoader : DefaultViewModelLoader
    {
        private readonly IWindsorContainer _container;

        public WindsorViewModelLoader(IWindsorContainer container)
        {
            this._container = container;
        }

        protected override object CreateViewModelInstance(Type viewModelType, IDotvvmRequestContext context)
        {
            return this._container.Resolve(viewModelType);
        }

        public override void DisposeViewModel(object instance)
        {
            
            this._container.Release(instance);
            base.DisposeViewModel(instance);
        }
    }
}