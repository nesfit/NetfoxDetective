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

namespace Netfox.Snoopers.SnooperICQ.Enums
{
    public enum ICQUserStatusVariousFlags
    {
        StatusWebawareFlag = 0x0001,
        StatusShowIPFlag = 0x0002,
        UserBirthdayFlag = 0x0008,
        UserActiveWebfrontFlag = 0x0020,
        DirectConnectionNotSupported = 0x0100,
        DirectConnectionUponAuthorization = 0x1000,
        DCOnlyWithContactUsers = 0x2000,
        Unknown
    }
}
