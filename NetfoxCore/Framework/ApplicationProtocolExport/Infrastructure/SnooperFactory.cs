using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Castle.Windsor;
using Netfox.Framework.ApplicationProtocolExport.Snoopers;
using Netfox.Framework.Models.Snoopers;

namespace Netfox.Framework.ApplicationProtocolExport.Infrastructure
{
    public class SnooperFactory : ISnooperFactory
    {
        public IWindsorContainer Container { get; }

        public SnooperFactory(IWindsorContainer container)
        {
            this.Container = container;
        }

        public ISnooper Create(ISnooper snooperPrototype, SelectedConversations conversations,
            DirectoryInfo exportDirectory, Boolean ignoreApplicationTags)
        {
            return this.Container.Resolve(snooperPrototype.GetType(), new Dictionary<string, object>
            {
                {"conversations", conversations},
                {"exportDirectory", exportDirectory},
                {"ignoreApplicationTags", ignoreApplicationTags}
            }) as ISnooper;
        }

        public ISnooper Create(Type snooperType, SelectedConversations conversations, DirectoryInfo exportDirectory,
            Boolean ignoreApplicationTags)
        {
            return this.Container.Resolve(snooperType, new Dictionary<string, object>()
            {
                {"conversations", conversations},
                {"exportDirectory", exportDirectory},
                {"ignoreApplicationTags", ignoreApplicationTags}
            }) as ISnooper;
        }

        public ISnooper Create(ISnooper snooperPrototype, IEnumerable<SnooperExportBase> sourceExports,
            DirectoryInfo exportDirectory)
        {
            return this.Container.Resolve(snooperPrototype.GetType(), new Dictionary<string, object>()
            {
                {"sourceExports", sourceExports as SnooperExportBase[] ?? sourceExports.ToArray()},
                {"exportDirectory", exportDirectory},
            }) as ISnooper;
        }

        public ISnooper Create(ISnooper snooperPrototype, IEnumerable<FileInfo> sourceFiles,
            DirectoryInfo exportDirectory)
        {
            return this.Container.Resolve(snooperPrototype.GetType(), new Dictionary<string, object>()
            {
                {"sourceFiles", sourceFiles as FileInfo[] ?? sourceFiles.ToArray()},
                {"exportDirectory", exportDirectory}
            }) as ISnooper;
        }

        public Type[] AvailableSnoopersTypes
            =>
                this.Container.Kernel.GetHandlers(typeof(ISnooper))
                    .SelectMany(h => h.ComponentModel.Services.Where(t => !t.IsInterface)).Distinct()
                    .ToArray();

        public ISnooper[] AvailableSnoopers
            => this.AvailableSnoopersTypes.Select(snooperType => this.Container.Resolve(snooperType) as ISnooper)
                .ToArray();
    }
}