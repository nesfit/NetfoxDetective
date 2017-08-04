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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Castle.Core.Logging;
using Netfox.Core.Interfaces;

namespace Netfox.Core
{
    public class SnooperLoader:IDetectiveService
    {
        public  IEnumerable<Assembly> GetSnoopersAssemblies()
        {
            var snoopers = new List<Assembly>();
            var enumerateFiles =
                Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "Snooper*.dll", SearchOption.AllDirectories)
                    .Concat(Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "*Snooper.dll", SearchOption.AllDirectories));
            foreach(var snooper in enumerateFiles)
            {
                try
                {
                    var snooperAssembly = Assembly.LoadFrom(snooper);
                    snoopers.Add(snooperAssembly);
                }
                catch(Exception ex)
                {
                    this.Logger?.Error($"Loading of {snooper} assembly have failed", ex);
                }
            }
            return snoopers;
        }

        #region Implementation of ILoggable
        public ILogger Logger { get; set; }
        #endregion
    }
}