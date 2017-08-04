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
using System.ComponentModel.DataAnnotations.Schema;

namespace Netfox.Framework.Models.Snoopers.Email
{
    /// <summary> Encapsulates the result of an email export.</summary>
    public class EmailExportResult : SnooperExportBase
    {
        private Type _dataType;

        /// <summary> Gets or sets the type of the data.</summary>
        /// <value> The type of the data.</value>
        [NotMapped]
        public Type DataType
        {
            get
            {
                try { return this._dataType ?? Type.GetType(this.DataTypeDb); }
                catch(Exception) {
                    return null;
                }
            }
            set { this._dataType = value;
                this.DataTypeDb = value.FullName;
            }
        }

        public string DataTypeDb { get; set; }

        /// <summary> Gets or sets the full pathname of the data file.</summary>
        /// <value> The full pathname of the data file.</value>
        public string DataPath { get; set; }

        /// <summary> Gets or sets the description.</summary>
        /// <value> The description.</value>
        public string Description { get; set; }
    }
}