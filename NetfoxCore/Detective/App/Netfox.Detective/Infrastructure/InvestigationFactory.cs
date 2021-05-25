using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Netfox.Core.Infrastructure;
using Netfox.Core.Interfaces;
using Netfox.Core.Models;
using Netfox.Detective.Interfaces;
using Netfox.Detective.Interfaces.Models.Base;
using Netfox.Detective.Models.Base;

namespace Netfox.Detective.Infrastructure
{
    internal class InvestigationFactory : IInvestigationFactory
    {
        public IWindsorContainer Container { get; }
        private IWindsorContainer chidWc;
        private IDetectiveMessenger _messenger;
        private ISerializationPersistor<Investigation> _investigationSerializationPersistor;

        public InvestigationFactory(IWindsorContainer container, IDetectiveMessenger messenger,
            ISerializationPersistor<Investigation> investigationSerializationPersistor)
        {
            this.Container = container;
            this._messenger = messenger;
            this._investigationSerializationPersistor = investigationSerializationPersistor;
        }

        #region Implementation of IInvestigationFactory

        public IInvestigation CreateInternal(IInvestigationInfo investigationInfo)
        {
            var chidWc =
                new WindsorContainer($"Investigation-{investigationInfo.InvestigationName}-{investigationInfo.Guid}",
                    new DefaultKernel(), new DefaultComponentInstaller());
            this.Container.AddChildContainer(chidWc);
            chidWc.Register(Component.For<IInvestigationInfo, InvestigationInfo>().Instance(investigationInfo));
            chidWc.Register(Component.For<IWindsorContainer, WindsorContainer>().Instance(chidWc));
            chidWc.Install(FromAssembly.InDirectory(new AssemblyFilter("."),
                new InstallerFactoryFilter(typeof(IDetectiveIvestigationWindsorInstaller))));
            var investigationInfoToReturn =
                chidWc.Resolve<IInvestigationFactoryInternal>().CreateInternal(investigationInfo);


            return investigationInfoToReturn;
        }

        public async Task<IInvestigation> Create(IInvestigationInfo investigationInfo)
        {
            chidWc = new WindsorContainer(
                $"Investigation-{investigationInfo.InvestigationName}-{investigationInfo.Guid}", new DefaultKernel(),
                new DefaultComponentInstaller());
            this.Container.AddChildContainer(chidWc);
            chidWc.Register(Component.For<IInvestigationInfo, InvestigationInfo>().Instance(investigationInfo));
            chidWc.Register(Component.For<IWindsorContainer, WindsorContainer>().Instance(chidWc));
            chidWc.Register(Component.For<IDetectiveMessenger>().Instance(this._messenger));
            chidWc.Register(Component.For<ISerializationPersistor<Investigation>>()
                .Instance(this._investigationSerializationPersistor));

            chidWc.Install(FromAssembly.InDirectory(new AssemblyFilter("."),
                new InstallerFactoryFilter(typeof(IDetectiveIvestigationWindsorInstaller))));

            foreach (var f in Directory.EnumerateFiles(".", "*Snooper*.WPF.dll"))
            {
                Assembly asm= Assembly.LoadFile(Path.GetFullPath(f));
                if (asm.GetTypes().Any(i => i.IsAssignableTo(typeof(IDetectiveIvestigationWindsorInstaller))))
                {
                    chidWc.Install(FromAssembly.Instance(asm,
                        new InstallerFactoryFilter(typeof(IDetectiveIvestigationWindsorInstaller))));
                }
            }

            var investigation = chidWc.Resolve<IInvestigationFactoryInternal>().CreateInternal(investigationInfo);

            investigationInfo.CreateFileStructure();
            await investigation.Initialize();

            return investigation;
        }

        #endregion
    }
}