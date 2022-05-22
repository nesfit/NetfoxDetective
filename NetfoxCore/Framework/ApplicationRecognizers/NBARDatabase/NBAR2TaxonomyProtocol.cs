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

namespace Netfox.NBARDatabase
{
    public partial class NBAR2TaxonomyProtocol
    {
        public Boolean Equals(NBAR2TaxonomyProtocol obj) => obj.name == this.name;
        // override object.Equals
        public override Boolean Equals(Object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if(obj == null || this.GetType() != obj.GetType()) { return false; }
            return this.Equals(obj as NBAR2TaxonomyProtocol);
        }

// override object.GetHashCode
        public override Int32 GetHashCode() => this.name.GetHashCode();
    }
}