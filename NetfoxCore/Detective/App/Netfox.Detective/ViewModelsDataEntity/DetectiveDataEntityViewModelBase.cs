using System;
using System.Threading.Tasks;
using Castle.Core;
using Castle.Windsor;
using Netfox.Core.Interfaces.ViewModels;
using Netfox.Detective.ViewModels;

namespace Netfox.Detective.ViewModelsDataEntity
{
    public abstract class DetectiveDataEntityViewModelBase : DetectiveViewModelBase, IDataEntityVm
    {
        protected DetectiveDataEntityViewModelBase(IWindsorContainer applicationOrInvestigationWindsorContainer) : base(
            applicationOrInvestigationWindsorContainer)
        {
        }

        protected DetectiveDataEntityViewModelBase(IWindsorContainer applicationOrInvestigationWindsorContainer,
            object model) : this(applicationOrInvestigationWindsorContainer)
        {
            this.EncapsulatedModel = model;
            if (model is IWindsorContainerChanger)
            {
                this.ApplicationOrInvestigationWindsorContainer =
                    (model as IWindsorContainerChanger).InvestigationWindsorContainer;
            }
        }

        public Type EncapsulatedDataType => this.EncapsulatedModel?.GetType();

        [DoNotWire] public object EncapsulatedModel { get; protected set; } //obsolete only for private use

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public virtual async Task Init()
        {
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }
}