using System;
using Netfox.Detective.ViewModels;

namespace Netfox.Detective.Interfaces
{
    public interface ICrossContainerHierarchyResolver
    {
        DetectiveViewModelBase Resolve(Type typeOfVm, object model);
        TVM Resolve<TVM>(object model) where TVM : class;
        ICrossContainerHierarchyResolver SubResolver { set; }
        Type[] AvailableAnalyzerTypes { get; }
    }
}