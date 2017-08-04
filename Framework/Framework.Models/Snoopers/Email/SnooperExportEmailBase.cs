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

using System.ComponentModel.DataAnnotations.Schema;
using System.IO;

namespace Netfox.Framework.Models.Snoopers.Email
{
    
    public class SnooperExportEmailBase: SnooperExportBase
    {
        [NotMapped] //todo fix
        public DirectoryInfo DirectoryInfo { get; private set; }
        protected SnooperExportEmailBase() { } //EF
        protected SnooperExportEmailBase(DirectoryInfo directoryInfo)
        {
            this.DirectoryInfo = directoryInfo;
        }
    }

    //public class SnooperExportEmailBaseTestEf : SnooperExportEmailBase
    //{
    //    public SnooperExportEmailBaseTestEf() : base(null) { }
    //    public SnooperExportEmailBaseTestEf(DirectoryInfo directoryInfo) : base(directoryInfo) { }
    //}
}
