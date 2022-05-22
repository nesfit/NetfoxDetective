using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core;
using Castle.Windsor;
using Netfox.Core.Interfaces.ViewModels;
using Netfox.Detective.Interfaces;
using Netfox.Detective.ViewModels;

namespace Netfox.Detective.Services
{
    public class CrossContainerHierarchyResolver : ICrossContainerHierarchyResolver
    {
        public IWindsorContainer Container { get; }

        public TVM Resolve<TVM>(object model) where TVM : class
        {
            return this.Resolve(typeof(TVM), model) as TVM;
        }

        [DoNotWire] public ICrossContainerHierarchyResolver SubResolver { private get; set; }

        public CrossContainerHierarchyResolver(IWindsorContainer container)
        {
            this.Container = container;
        }

        public DetectiveViewModelBase Resolve(Type typeOfVm, object model)
        {
            if (this.Container.Kernel.GetHandler(typeOfVm) != null)
            {
                return this.Container.Resolve(typeOfVm, new Dictionary<string, object>
                {
                    {"model", model}
                }) as DetectiveViewModelBase;
            }

            return this.SubResolver?.Resolve(typeOfVm, model);
        }

        public Type[] AvailableAnalyzerTypes => this.Container.Kernel.GetHandlers(typeof(IAnalyzer))
            .Concat(this.Container.Kernel.GetHandlers(typeof(IAnalyzerInvestigation)))
            .Concat(this.Container.Kernel.GetHandlers(typeof(IAnalyzerApplication)))
            .SelectMany(h => h.ComponentModel.Services.Where(t => !t.IsInterface))
            .Concat(this.SubResolver?.AvailableAnalyzerTypes ?? new Type[0])
            .Distinct()
            .ToArray();
    }
}