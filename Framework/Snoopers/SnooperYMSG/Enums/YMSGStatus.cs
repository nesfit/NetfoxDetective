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

namespace Netfox.SnooperYMSG.Enums
{
    public enum YMSGStatus
    {
        YAHOO_STATUS_AVAILABLE = 0,
        YAHOO_STATUS_BRB = 1,
        YAHOO_STATUS_BUSY = 2,
        YAHOO_STATUS_NOTATHOME = 3,
        YAHOO_STATUS_NOTATDESK = 4,
        YAHOO_STATUS_NOTINOFFICE = 5,
        YAHOO_STATUS_ONVACATION = 7,
        YAHOO_STATUS_OUTTOLUNCH = 8,
        YAHOO_STATUS_STEPPEDOUT = 9,
        YAHOO_STATUS_CUSTOM = 99,
        YAHOO_STATUS_IDLE = 999,
        YAHOO_STATUS_WEBLOGIN = 1515563605,
        YAHOO_STATUS_OFFLINE = 1515563606,
        YAHOO_STATUS_TYPING = 22,
        Unknown
    }
}