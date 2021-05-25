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
using System.Linq;
using System.Text;
using Netfox.Snoopers.SnooperWebmails.Models.Spotters;
using Netfox.Snoopers.SnooperWebmails.Models.WebmailEvents;

namespace Netfox.Snoopers.SnooperWebmails.Models.SpotterVisitors
{
    /// <summary>
    /// Implementation of method that gets message from seznam webmail event New Message.
    /// Returns MailMsg object.
    /// </summary>
    class SeznamGetNewMessage : ISpotterVisitor
    {
        #region Implementation of ISpotterVisitor
        public object applyOn(SpotterJson spotter) { throw new NotImplementedException(); }
        public object applyOn(SpotterKeyValue spotter) { throw new NotImplementedException(); }
        public object applyOn(SpotterMultipart spotter) { throw new NotImplementedException(); }
        public object applyOn(SpotterText spotter) { throw new NotImplementedException(); }

        public object applyOn(SpotterFRPC spotter)
        {

            var msg = new MailMsg();

            foreach(var item in spotter.Items)
            {
                var methodCall = item as FRPCparser.FRPCMethodCall;
                if(methodCall == null || !methodCall.Name.Equals("user.message.send")) { continue; }
                foreach(var param in methodCall.Parameters)
                {
                    var email = param as FRPCparser.FRPCStruct;
                    if(email == null) { continue; }

                    foreach(var it in email.Items)
                    {
                        if(it.Key.Equals("subject"))
                        {
                            var itSubj = it.Value as FRPCparser.FRPCString;
                            msg.Subject = itSubj?.Value;
                        }
                        else if(it.Key.Equals("body"))
                        {
                            var itBody = it.Value as FRPCparser.FRPCString;
                            msg.Body = itBody?.Value;
                        }
                        else if(it.Key.Equals("to"))
                        {
                            var recips = it.Value as FRPCparser.FRPCArray;
                            if(recips != null) msg.To = GetAddresses(recips);
                        }
                        else if(it.Key.Equals("from"))
                        {
                            var senders = it.Value as FRPCparser.FRPCArray;
                            if(senders != null) msg.From = GetAddresses(senders);
                        }
                    }
                }
            }

            return msg;
        }
        #endregion

        public static string GetAddresses(FRPCparser.FRPCArray recips)
        {
            var sb = new StringBuilder();
            foreach (var recip in recips.Items)
            {
                var recipStruct = recip as FRPCparser.FRPCStruct;
                if (recipStruct == null) { continue; }
                foreach (var addr in from r in recipStruct.Items
                                     where r.Key.Equals("email")
                                     select r.Value as FRPCparser.FRPCString)
                { sb.Append(addr?.Value + "; "); }
            }
            return sb.ToString();
        }
    }
}
