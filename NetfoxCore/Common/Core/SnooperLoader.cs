using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Castle.Core.Logging;
using Netfox.Core.Interfaces;

namespace Netfox.Core
{
    public class SnooperLoader : IDetectiveService
    {
        public IEnumerable<Assembly> GetSnoopersAssemblies()
        {
            var snoopers = new List<Assembly>();
            var enumerateFiles =
                Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "Snooper*.dll",
                        SearchOption.AllDirectories)
                    .Concat(Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "*Snooper.dll",
                        SearchOption.AllDirectories));
            foreach (var snooper in enumerateFiles)
            {
                try
                {
                    var snooperAssembly = Assembly.LoadFrom(snooper);
                    snoopers.Add(snooperAssembly);
                }
                catch (Exception ex)
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