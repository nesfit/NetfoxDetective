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
using System.IO;

namespace Netfox.Snoopers.SnooperMAFF.Models.Common
{
    /// <summary>
    /// Class desribes load configuration from file, if it enabled by settings visualisation
    /// </summary>
    public class Config
    {
        private const string GenerateSnapshots = "[GenerateSnapshots] ";
        private const string SnapshotsTimeSeparator = "[SnapshotsTimeSeparator] ";
        private const string ObjectRewrite = "[ObjectRewrite] ";
      
        /// <summary>
        /// Initializes a new instance of the <see cref="Config"/> class.
        /// </summary>
        /// <param name="oExportDirectory">The o export directory.</param>
        public Config(DirectoryInfo oExportDirectory)
        {
            this.ReadDataFromFile(oExportDirectory);
        }

        /// <summary>
        /// Reads the data from file.
        /// </summary>
        /// <param name="oExportDirectory">The export directory.</param>
        private void ReadDataFromFile(DirectoryInfo oExportDirectory)
        {
            if (!File.Exists(oExportDirectory + @"\config.conf")) { return; }

            var arrayOfLines = File.ReadAllLines(oExportDirectory + @"\config.conf");

            foreach(var sLine in arrayOfLines)
            {
                this.CheckCurrentCommand(sLine);        
            }
        }

        /// <summary>
        /// Checks the current command and proceed his value.
        /// </summary>
        /// <param name="sLine">The line string.</param>
        private void CheckCurrentCommand(string sLine)
        {
            if (sLine.Contains("///")) { return; }
            var iBeginPosition = sLine.IndexOf(GenerateSnapshots, StringComparison.Ordinal);
            if(iBeginPosition >= 0)
            {
                iBeginPosition += GenerateSnapshots.Length;
                var sValue = sLine.Substring(iBeginPosition, sLine.Length - iBeginPosition);
                if (sValue.Equals("true")) { Constants.GenerateSnapshots = true; }
                else if (sValue.Equals("false")) { Constants.GenerateSnapshots = false; }
                return;
            }

            iBeginPosition = sLine.IndexOf(SnapshotsTimeSeparator, StringComparison.Ordinal);
            if (iBeginPosition >= 0)
            {
                iBeginPosition += SnapshotsTimeSeparator.Length;
                long ulNumber;
                if (Int64.TryParse(sLine.Substring(iBeginPosition, sLine.Length - iBeginPosition), out ulNumber))
                {
                    if (ulNumber < 100) { return; }
                    Constants.SnapshotsTimeSeparator = ulNumber;
                }
                return;
            }

            iBeginPosition = sLine.IndexOf(ObjectRewrite, StringComparison.Ordinal);
            if (iBeginPosition >= 0)
            {
                iBeginPosition += ObjectRewrite.Length;
                var sValue = sLine.Substring(iBeginPosition, sLine.Length - iBeginPosition);
                if (sValue.Equals("true")) { Constants.ObjectRewrite = true; }
                else if (sValue.Equals("false")) { Constants.ObjectRewrite= false; }
            }
        }
    }
}
