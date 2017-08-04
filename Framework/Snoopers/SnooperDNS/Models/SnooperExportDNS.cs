// Copyright (c) 2017 Jan Pluskal, Pavol Vican
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
using System.Text;
using Netfox.Framework.Models.Snoopers;

namespace Netfox.SnooperDNS.Models
{
    public class SnooperExportDNS : SnooperExportBase
    {
        public SnooperExportDNS() : base() { } //EF

        #region Overrides of SnooperExportBase
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(base.ToString());
            foreach (var exportedObject in this.ExportObjects)
            {
                sb.AppendLine(exportedObject.ToString());
                //this.PrintSnooperExportedObject(exportedObject);
                //sb.AppendLine(exportedObject.ToString());
                sb.AppendLine("    object:");
                sb.AppendLine("      timestamp: " + exportedObject.TimeStamp);
                if (exportedObject.Reports != null)
                {
                    sb.AppendLine("      reports: " + exportedObject.Reports.Count);
                }
                else
                {
                    sb.AppendLine("      reports: 0");
                }

                sb.Append("      validity: ");
                switch (exportedObject.ExportValidity)
                {
                    case Netfox.Core.Enums.ExportValidity.ValidWhole:
                        sb.AppendLine("valid (whole)");
                        break;
                    case Netfox.Core.Enums.ExportValidity.ValidFragment:
                        sb.AppendLine("valid (fragment)");
                        break;
                    case Netfox.Core.Enums.ExportValidity.Malformed:
                        sb.AppendLine("malformed");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                /*var exportedDataObject = exportedObject as SnooperExportedDataObjectFTP;
                sb.AppendLine("      - command: " + exportedDataObject?.Command);
                if (!string.IsNullOrEmpty(exportedDataObject?.Value)) { sb.AppendLine("      - value: " + exportedDataObject?.Value); }*/
            }
            return sb.ToString();
        }
        #endregion
    }
}
