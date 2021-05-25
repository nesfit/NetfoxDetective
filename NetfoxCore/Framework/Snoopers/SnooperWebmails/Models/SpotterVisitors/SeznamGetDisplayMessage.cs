// Copyright (c) 2017 Jan Pluskal, Miroslav Slivka
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
using Netfox.Snoopers.SnooperWebmails.Models.Spotters;
using Netfox.Snoopers.SnooperWebmails.Models.WebmailEvents;

namespace Netfox.Snoopers.SnooperWebmails.Models.SpotterVisitors
{
    class SeznamGetDisplayMessage : ISpotterVisitor
    {
        #region Implementation of ISpotterVisitor
        public object applyOn(SpotterJson spotter) { throw new NotImplementedException(); }
        public object applyOn(SpotterKeyValue spotter) { throw new NotImplementedException(); }
        public object applyOn(SpotterMultipart spotter) { throw new NotImplementedException(); }
        public object applyOn(SpotterText spotter) { throw new NotImplementedException(); }

        public object applyOn(SpotterFRPC spotter)
        {
            var mailMsg = new MailMsg();
            foreach (var item in spotter.Items)
            {
                var methodResponse = item as FRPCparser.FRPCMethodRespone;
                if (methodResponse == null) continue;

                foreach (var data in methodResponse.Data)
                {
                    var msgStruct = data as FRPCparser.FRPCStruct;
                    if (msgStruct == null) continue;

                    foreach (var msgItem in msgStruct.Items)
                    {
                        if (!msgItem.Key.Equals("message")) continue;
                        var msg = msgItem.Value as FRPCparser.FRPCStruct;
                        if (msg == null) continue;

                        foreach (var mailField in msg.Items)
                        {
                            if (mailField.Key.Equals("subject"))
                            {
                                var itSubj = mailField.Value as FRPCparser.FRPCString;
                                mailMsg.Subject = itSubj?.Value;
                            }
                            else if (mailField.Key.Equals("body"))
                            {
                                var itBody = mailField.Value as FRPCparser.FRPCString;
                                mailMsg.Body = itBody?.Value;
                            }
                            else if (mailField.Key.Equals("to"))
                            {
                                var recips = mailField.Value as FRPCparser.FRPCArray;
                                if (recips != null) mailMsg.To = SeznamGetNewMessage.GetAddresses(recips);
                            }
                            else if (mailField.Key.Equals("from"))
                            {
                                var senders = mailField.Value as FRPCparser.FRPCArray;
                                if (senders != null) mailMsg.From = SeznamGetNewMessage.GetAddresses(senders);
                            }
                            else if (mailField.Key.Equals("folder"))
                            {
                                var folder = mailField.Value as FRPCparser.FRPCInteger8;
                                if (folder != null) mailMsg.SourceFolder = folder.Value.ToString();
                            }

                        }

                    }

                }

            }

            return mailMsg;
        }
        #endregion
    }
}
