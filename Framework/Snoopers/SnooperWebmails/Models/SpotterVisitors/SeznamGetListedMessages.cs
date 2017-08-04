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
using System.Collections.Generic;
using Netfox.SnooperWebmails.Models.Spotters;
using Netfox.SnooperWebmails.Models.WebmailEvents;

namespace Netfox.SnooperWebmails.Models.SpotterVisitors
{
    /// <summary>
    /// Implementation of method that gets messages from seznam webmail event List Folder.
    /// Returns List of MailMsg.
    /// </summary>
    class SeznamGetListedMessages : ISpotterVisitor
    {
        #region Implementation of ISpotterVisitor
        public object applyOn(SpotterJson spotter) { throw new NotImplementedException(); }
        public object applyOn(SpotterKeyValue spotter) { throw new NotImplementedException(); }
        public object applyOn(SpotterMultipart spotter) { throw new NotImplementedException(); }
        public object applyOn(SpotterText spotter) { throw new NotImplementedException(); }

        public object applyOn(SpotterFRPC spotter)
        {
            var list = new List<MailMsg>();
            foreach(var item in spotter.Items)
            {
                var methodResponse = item as FRPCparser.FRPCMethodRespone;
                if (methodResponse == null) continue;

                foreach(var data in methodResponse.Data)
                {
                    var msgStruct = data as FRPCparser.FRPCStruct;
                    if(msgStruct == null) continue;

                    foreach(var msgItem in msgStruct.Items)
                    {
                        if(!msgItem.Key.Equals("messages")) continue;
                        var msgArray = msgItem.Value as FRPCparser.FRPCArray;
                        if(msgArray == null) continue;

                        foreach(var msg in msgArray.Items)
                        {
                            var mailMsg = new MailMsg();
                            var email = msg as FRPCparser.FRPCStruct;
                            if (email == null) continue;
                            foreach(var it in email.Items)
                            {
                                if (it.Key.Equals("subject"))
                                {
                                    var itSubj = it.Value as FRPCparser.FRPCString;
                                    mailMsg.Subject = itSubj?.Value;
                                }
                                else if (it.Key.Equals("abstract"))
                                {
                                    var itBody = it.Value as FRPCparser.FRPCString;
                                    mailMsg.Body = itBody?.Value;
                                }
                                else if (it.Key.Equals("to"))
                                {
                                    var recips = it.Value as FRPCparser.FRPCArray;
                                    if (recips != null) mailMsg.To = SeznamGetNewMessage.GetAddresses(recips);
                                }
                                else if (it.Key.Equals("from"))
                                {
                                    var senders = it.Value as FRPCparser.FRPCArray;
                                    if (senders != null) mailMsg.From = SeznamGetNewMessage.GetAddresses(senders);
                                }
                                else if(it.Key.Equals("folder"))
                                {
                                    var folder = it.Value as FRPCparser.FRPCInteger8;
                                    if(folder != null) mailMsg.SourceFolder = folder.Value.ToString();
                                }
                            }

                            list.Add(mailMsg);

                        }

                    }

                }
            }

            return list;
        }
        #endregion
    }
}
