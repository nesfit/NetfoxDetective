// Copyright (c) 2017 Jan Pluskal, Miroslav Slivka, Viliam Letavay
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

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

        public SnooperFactory(IWindsorContainer container) { this.Container = container; }

        public ISnooper Create(ISnooper snooperPrototype,SelectedConversations conversations, DirectoryInfo exportDirectory, Boolean ignoreApplicationTags)
        {
            return this.Container.Resolve(snooperPrototype.GetType(), new
            {
                conversations,
                exportDirectory,
                ignoreApplicationTags
            }) as ISnooper;
        }
        public ISnooper Create(Type snooperType, SelectedConversations conversations, DirectoryInfo exportDirectory, Boolean ignoreApplicationTags)
        {
            return this.Container.Resolve(snooperType, new
            {
                conversations,
                exportDirectory,
                ignoreApplicationTags
            }) as ISnooper;
        }
        public ISnooper Create(ISnooper snooperPrototype, IEnumerable<SnooperExportBase> sourceExports, DirectoryInfo exportDirectory)
        {
            return this.Container.Resolve(snooperPrototype.GetType(), new
            {
                sourceExports = sourceExports as SnooperExportBase[] ?? sourceExports.ToArray(),
                exportDirectory,
            }) as ISnooper;
        }
        public ISnooper Create(ISnooper snooperPrototype, IEnumerable<FileInfo> sourceFiles, DirectoryInfo exportDirectory)
        {
            return this.Container.Resolve(snooperPrototype.GetType(), new
            {
                sourceFiles = sourceFiles as FileInfo[] ?? sourceFiles.ToArray(),
                exportDirectory
            }) as ISnooper;
        }

        public Type[] AvailableSnoopersTypes
            =>
                    this.Container.Kernel.GetHandlers(typeof(ISnooper))
                        .SelectMany(h => h.ComponentModel.Services.Where(t => !t.IsInterface)).Distinct()
                        .ToArray();

        public ISnooper[] AvailableSnoopers
            =>  this.AvailableSnoopersTypes.Select(snooperType =>this.Container.Resolve(snooperType) as ISnooper).ToArray();
    }
}