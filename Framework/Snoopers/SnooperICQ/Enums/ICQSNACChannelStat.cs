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
    public enum ICQSNACChannelStat
    {
        LOGON = 0x01,
        SNAC = 0x02,
	    FLAP_LVL_ERROR =0x03,
	    LOGOUT = 0x04,
	    KEEP_ALIVE = 0x05,
        Unknown
    }
}
