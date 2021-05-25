using System.Linq;
using Castle.Facilities.Startable;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Netfox.Framework.CaptureProcessor.ApplicationRecognizer;
using Netfox.Framework.CaptureProcessor.Captures;
using Netfox.Framework.CaptureProcessor.CoreController;
using Netfox.Framework.CaptureProcessor.Interfaces;
using Netfox.Framework.CaptureProcessor.L3L4ConversationTracking;
using Netfox.Framework.CaptureProcessor.L7Tracking;
using Netfox.Framework.CaptureProcessor.L7Tracking.TCP;
using Netfox.Framework.CaptureProcessor.L7Tracking.UDP;
using Netfox.Framework.Models;
using Netfox.Framework.Models.Interfaces;
using Netfox.Nbar;
using Netfox.NBARDatabase;
using Netfox.RTP;

namespace Netfox.Framework.CaptureProcessor.Infrastructure
{
    public class CaptureProcessorWindsorInstaller: IWindsorInstaller
    {
        #region Implementation of IWindsorInstaller
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            this.AddFacilities(container);
            this.FactoriesRegister(container);
            this.OtherTypesRegister(container);
        }

        internal void OtherTypesRegister(IWindsorContainer container)
        {
            container.Register(Component.For<L3L4ConversationTrackerBlock>().LifestyleTransient());
            container.Register(Component.For<L7ConversationTrackerBlock>().LifestyleTransient());
            container.Register(Component.For<ControllerCaptureProcessorLocal>().LifestyleSingleton());
            container.Register(Component.For<IApplicationRecognizer, ApplicationRecognizerDefault>().LifestyleSingleton());
            container.Register(Component.For<ApplicationRecognizerRTP>().OnlyNewServices().LifestyleSingleton());
            container.Register(Component.For<ApplicationRecognizerNBAR>().OnlyNewServices().LifestyleSingleton());
            container.Register(Component.For<NBARProtocolPortDatabase>().OnlyNewServices().LifestyleSingleton());
            container.Register(Component.For<PmCaptureProcessorBlockMnm>().LifestyleTransient());
            container.Register(Component.For<PmCaptureProcessorBlockPcap>().LifestyleTransient());
            container.Register(Component.For<PmCaptureProcessorBlockPcapNg>().LifestyleTransient());
            container.Register(Component.For<CaptureProcessorBlock>().LifestyleTransient());
            container.Register(Component.For<L3ConversationExtended>().LifestyleTransient());
            container.Register(Component.For<L4ConversationExtended>().LifestyleTransient());
            container.Register(Component.For<L7Conversation>().LifestyleTransient());
            container.Register(Component.For<TCPTracker>().LifestyleTransient());
            container.Register(Component.For<UDPTracker>().LifestyleTransient());
            container.Register(Component.For<FlowStore>().LifestyleTransient());
        }

        internal void FactoriesRegister(IWindsorContainer container)
        {
            container.Register(Component.For<IControllerCaptureProcessorFactoryInternal>().AsFactory());
            container.Register(Component.For<IFlowStoreFactory>().AsFactory());
            container.Register(Component.For<IL7ConversationFactory>().AsFactory());
            container.Register(Component.For<IL4ConversationFactory>().AsFactory());
            container.Register(Component.For<IL3ConversationFactory>().AsFactory());
            container.Register(Component.For<IL7ConversationTrackerBlockFactory>().AsFactory());
            container.Register(Component.For<IControllerCaptureProcessorFactory>().AsFactory());
            container.Register(Component.For<IPmCaptureProcessorBlockFactory>().AsFactory());
            container.Register(Component.For<INetfoxDBContextFactory>().AsFactory());
            container.Register(Component.For<IL3L4ConversationTrackerBlockFactory>().AsFactory());
            container.Register(Component.For<ICaptureProcessorBlockFactory>().AsFactory());
            container.Register(Component.For<IL7ConversationTrackerFactory>().AsFactory());
        }

        internal void AddFacilities(IWindsorContainer container)
        {
            if (!container.Kernel.GetFacilities().Any(f => f is StartableFacility)) { container.AddFacility<StartableFacility>(); }
            if (!container.Kernel.GetFacilities().Any(f => f is TypedFactoryFacility)) { container.AddFacility<TypedFactoryFacility>(); }
        }
        #endregion
    }
}