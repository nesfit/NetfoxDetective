// Copyright (c) 2017 Jan Pluskal
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Netfox.Framework.ApplicationProtocolExport.Infrastructure;
using Netfox.Framework.ApplicationProtocolExport.Snoopers;
using Netfox.Framework.Models.Interfaces;
using Netfox.Framework.Models.Snoopers;
using Netfox.NetfoxFrameworkAPI.Interfaces;
using Netfox.Framework.CaptureProcessor.Interfaces;


namespace Netfox.NetfoxFrameworkAPI
{
    public class FrameworkController : IFrameworkController
    {
        public FrameworkController(IControllerCaptureProcessorFactory controllerCaptureProcessorFactory, ISnooperFactory snooperFactory)
        {
            this.ControllerCaptureProcessorFactory = controllerCaptureProcessorFactory;
            this.SnooperFactory = snooperFactory;
        }

        public void ProcessCapture(FileInfo captureFileInfo)
        {
            Debugger.Log(0, "Debug", "###NFX### FRAMEWORK CONTROLLER STARTING CAPTURE");
            var controllerCaptureProcessor = this.ControllerCaptureProcessorFactory.Create();
            controllerCaptureProcessor.ProcessCapture(captureFileInfo);
        }

        #region Operation methods
        public void ExportData(
            IEnumerable<ISnooper> snoopers,
            IEnumerable<ILxConversation> conversations,
            DirectoryInfo exportDirectory,
            
            Boolean ignoreApplicationTags)
        {
            if(conversations == null) { conversations = new List<ILxConversation>(); }
            if(snoopers == null) { snoopers = new List<ISnooper>(); }

            var l7Conversations = conversations as ILxConversation[] ?? conversations.ToArray();
            var selectedConversations = new SelectedConversations(l7Conversations);
            foreach(var snooperPrototype in snoopers)
            {
                var snooper = this.SnooperFactory.Create(snooperPrototype, selectedConversations, exportDirectory, ignoreApplicationTags);
                snooper.Run();
            }
        }
        [Obsolete]
        public void ExportData(
            IEnumerable<Type> snooperTypes,
            IEnumerable<ILxConversation> conversations,
            DirectoryInfo exportDirectory,
            
            Boolean ignoreApplicationTags)
        {
            if(conversations == null) { conversations = new List<ILxConversation>(); }
            if(snooperTypes == null) { snooperTypes = new List<Type>(); }

            var lxConversations = conversations as ILxConversation[] ?? conversations.ToArray();
            var selectedConversations = new SelectedConversations(lxConversations);
            foreach(var snooperType in snooperTypes)
            {
                var snooper = this.SnooperFactory.Create(snooperType,
                    selectedConversations,
                    exportDirectory,
                    ignoreApplicationTags
                );
                snooper.Run();
            }
        }

        public void ExportData(IEnumerable<ISnooper> snoopers, IEnumerable<SnooperExportBase> snooperExports, DirectoryInfo exportDirectory)
        {
            if(snoopers == null) { snoopers = new List<ISnooper>(); }
            if(snooperExports == null) { snooperExports = new List<SnooperExportBase>(); }

            var snooperExportBases = snooperExports.ToArray();
            foreach(var snooperPrototype in snoopers)
            {
                var snooper = this.SnooperFactory.Create(snooperPrototype, snooperExportBases,
                    exportDirectory
                ) ;
                snooper.Run();
            }
        }

        public void ExportData(IEnumerable<ISnooper> snoopers, ConcurrentBag<SnooperExportBase> snooperExports, DirectoryInfo exportDirectory)
        {
            if(snoopers == null) { snoopers = new List<ISnooper>(); }
            if(snooperExports == null) { snooperExports = new ConcurrentBag<SnooperExportBase>(); }
            foreach(var snooperPrototype in snoopers)
            {
                var snooper = this.SnooperFactory.Create(snooperPrototype,snooperExports,
                    exportDirectory
                );
                snooper.Run();
            }
        }

        public void ExportData(IEnumerable<ISnooper> snoopers, IEnumerable<FileInfo> sourceFiles, DirectoryInfo exportDirectory)
        {
            if(snoopers == null) { snoopers = new List<ISnooper>(); }
            if(sourceFiles == null) { sourceFiles = new List<FileInfo>(); }

            var fileInfos = sourceFiles as FileInfo[] ?? sourceFiles.ToArray();
            foreach(var snooperPrototype in snoopers)
            {
                var snooper = this.SnooperFactory.Create(snooperPrototype, fileInfos,
                    exportDirectory
                );
                snooper.Run();
            }
        }
        #endregion

        #region Dependency objects
        internal IControllerCaptureProcessorFactory ControllerCaptureProcessorFactory { get; }
        public ISnooperFactory SnooperFactory { get; }
        #endregion
    }
}