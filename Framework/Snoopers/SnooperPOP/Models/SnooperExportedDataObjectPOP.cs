// Copyright (c) 2017 Jan Pluskal, Martin Mares
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

using System.Text;
using Netfox.Framework.Models.Snoopers;
using Netfox.SnooperPOP.Interfaces;

namespace Netfox.SnooperPOP.Models
{
    public class SnooperExportedDataObjectPOP : SnooperExportedObjectBase, IExportPOP
    {
        public string Type { get; set; }
        public string Value { get; set; }
        private SnooperExportedDataObjectPOP() : base() { } //EF
        public SnooperExportedDataObjectPOP(SnooperExportBase exportBase) : base(exportBase)
        {
            this.Type = string.Empty;
            this.Value = string.Empty;
        }

        #region Overrides of Object
        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            //sb.AppendLine(base.ToString());
            sb.AppendLine("type: " + this.Type);
            sb.Append("value: " + this.Value);
            return sb.ToString();
        }
        #endregion
    }
}
