// Copyright (c) 2017 Jan Pluskal, Vit Janecek
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
using System.Diagnostics;
using Netfox.SnooperMAFF.Models.Objects;

namespace Netfox.SnooperMAFF.WPF.View.Events
{
    /// <summary>
    /// Static class defines opening file in his native format by running his native program.
    /// </summary>
    static class OpenFileEvent
    {
        /// <summary>
        /// Open and run file in native program.
        /// </summary>
        /// <param name="oArchiveObjectobject">The archive selected object.</param>
        public static void OpenEvent(BaseObject oArchiveObjectobject)
        {
            if (oArchiveObjectobject == null) { return; }
            try
            {
                var sPath = Environment.GetEnvironmentVariable("temp");
                Process.Start(sPath + @"\MAFF\" + oArchiveObjectobject.ArchiveBaseFolder + @"\" + oArchiveObjectobject.FileName.Replace("/", @"\"));
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nMessage ---\n{0}", ex.Message);
                Console.WriteLine("\nHelpLink ---\n{0}", ex.HelpLink);
                Console.WriteLine("\nSource ---\n{0}", ex.Source);
                Console.WriteLine("\nStackTrace ---\n{0}", ex.StackTrace);
                Console.WriteLine("\nTargetSite ---\n{0}", ex.TargetSite);
            }
        }
    }
}
