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

namespace Netfox.SnooperICQ.Enums
{
    public enum ICQUserStatus
    {
        online = 0x0000,
        away = 0x0001,
        doNotDisturb = 0x0002,
	    notAvailable = 0x0004,
	    occupied_1 = 0x0010,
	    occupied_2 = 0x0011,
	    freeForChat = 0x0020,
	    invisible_1 = 0x0100,
	    invisible_2 = 0x0111,
        Unknown
    }
}
