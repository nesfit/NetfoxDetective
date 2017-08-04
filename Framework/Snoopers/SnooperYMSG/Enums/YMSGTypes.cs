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
    public enum YMSGTypes
    {
        YAHOO_SERVICE_LOGON = 0x01,
        YAHOO_SERVICE_LOGOFF = 0x02,
        YAHOO_SERVICE_MESSAGE = 0x06,
        YAHOO_SERVICE_PING = 0x12,
        YAHOO_SERVICE_SKINNAME = 0x15,
        YAHOO_SERVICE_HANDSHAKE = 0x4C,
        YAHOO_SERVICE_AUTHRESP = 0x54,
        YAHOO_SERVICE_LIST = 0x55,
        YAHOO_SERVICE_AUTH = 0x57,
        YAHOO_SERVICE_KEEP_ALIVE = 0x8a,
        YAHOO_SERVICE_NOTIFY = 0x4b,
        YAHOO_SERVICE_Y7_CHAT_SESSION = 0xd4,
        YAHOO_SERVICE_LIST_V15 = 0xf1,
        YAHOO_SERVICE_ADD_BUDDY = 0x83,
        YAHOO_SERVICE_Y7_BUDDY_AUTH = 0xd6,
        YAHOO_SERVICE_STATUS_V15 = 0xf0,
        YAHOO_SERVICE_Y7_STATUS_UPDATE = 0xc6,
        YAHOO_SERVICE_Y7_FILE_TRANSFER = 0xdc,
        YAHOO_SERVICE_Y7_FILE_TRANSFER_INFORMATION = 0xdd,
        YAHOO_SERVICE_Y7_FILE_TRANSFER_ACCEPT = 0xde,
        YAHOO_SERVICE_Y7_CONTACT_DETAILS = 0xd3,
        Unknown
    }
}