// Copyright (c) 2017 Jan Pluskal, Filip Karpisek
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

using Netfox.Framework.Models.Snoopers;

namespace Netfox.Snoopers.SnooperBTC.Models
{
	public class SnooperExportBTC : SnooperExportBase
	{
        public SnooperExportBTC() : base() { } //EF
                                                 /*public override string ToString()
                                                 {
                                                     {
                                                         var sb = new StringBuilder();
                                                         sb.AppendLine(base.ToString());
                                                         if(this.ExportObjects != null)
                                                         {
                                                             foreach(var exportedObject in this.ExportObjects)
                                                             {
                                                                 //this.PrintSnooperExportedObject(exportedObject);
                                                                 //sb.AppendLine(exportedObject.ToString());
                                                                 sb.AppendLine("    object:");
                                                                 sb.AppendLine("      " + exportedObject.ToString());
                                                                 sb.AppendLine("      timestamp: " + exportedObject.TimeStamp);
                                                                 if(exportedObject.Reports != null) { sb.AppendLine("      reports: " + exportedObject.Reports.Count); }
                                                                 else
                                                                 { sb.AppendLine("      reports: 0"); }

                                                                 sb.Append("      validity: ");
                                                                 switch(exportedObject.ExportValidity)
                                                                 {
                                                                     case ExportValidity.ValidWhole:
                                                                         sb.AppendLine("valid (whole)");
                                                                         break;
                                                                     case ExportValidity.ValidFragment:
                                                                         sb.AppendLine("valid (fragment)");
                                                                         break;
                                                                     case ExportValidity.Malformed:
                                                                         sb.AppendLine("malformed");
                                                                         break;
                                                                     default:
                                                                         throw new ArgumentOutOfRangeException();
                                                                 }
                                                             }
                                                         }
                                                         return sb.ToString();
                                                     }
                                                 }*/
    }
}