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
using System.Xml.XPath;
using Netfox.SnooperWebmails.Models.Spotters;
using Netfox.SnooperWebmails.Models.WebmailEvents;

namespace Netfox.SnooperWebmails.Models.SpotterVisitors
{
    class YahooGetDisplayMessage : ISpotterVisitor
    {
        #region Implementation of ISpotterVisitor
        public object applyOn(SpotterJson spotter)
        {
            var root = spotter.GetRoot();
            var mail = new MailMsg();
            if (root.XPathSelectElement("/result/message") != null)
            {
                // there should be only one message in response to display message
                foreach (var msg in root.XPathSelectElements("/result/message/item"))
                {
                    mail.Subject = msg.XPathSelectElement("header/subject").Value;
                    mail.From = msg.XPathSelectElement("header/from/email").Value;
                    mail.To = YahooGetListedMessages.GetAllInArray(msg, "header/to//email", "; ");
                    mail.Cc = YahooGetListedMessages.GetAllInArray(msg, "header/cc//email", "; ");
                    mail.Bcc = YahooGetListedMessages.GetAllInArray(msg, "header/bcc//email", "; ");
                    mail.Body = YahooGetListedMessages.GetAllInArray(msg, "part//text", "");
                    mail.SourceFolder = msg.XPathSelectElement("sourceFolderInfo/name").Value;
                }
            }
            
            return mail;
        }
        public object applyOn(SpotterKeyValue spotter) { throw new NotImplementedException(); }

        public object applyOn(SpotterMultipart spotter)
        {
            spotter.Reset();
            while (spotter.HasNextPart())
            {
                var s = spotter.GetNextPart();
                var o = s.Accept(this);
                var part = o as MailMsg;
                if (part != null && !part.IsEmpty) return part;
                spotter.UnloadPart(s);
            }

            return new MailMsg();
        }
        public object applyOn(SpotterText spotter) { throw new NotImplementedException(); }
        public object applyOn(SpotterFRPC spotter) { throw new NotImplementedException(); }
        #endregion
    }
}
