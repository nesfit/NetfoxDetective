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

namespace Netfox.SnooperMAFF.Models.Common
{
    /// <summary>
    /// Static class desribes constant for configuring snooper parsing
    /// </summary>
    public static class Constants
    {
        //Default constants
        //Constant values might be changed by reading data from config file

        //Bool determines if Generate snapshot function will be enable
        public static bool GenerateSnapshots = true;
        
        //Snapshots Separator determines how many miliseconds can be minimal time difference between archive snapshot parts
        public static long SnapshotsTimeSeparator = 500;

        //Bool determines if same object will be updated (rewrite) or added with tag (number) before number
        public static bool ObjectRewrite = true;
    }
}
