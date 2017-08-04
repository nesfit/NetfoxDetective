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
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using Netfox.SnooperWebmails.Models.Spotters;
using Netfox.SnooperWebmails.Models.WebmailEvents;

namespace Netfox.SnooperWebmails.Models.SpotterVisitors
{
    /// <summary>
    /// Implementation of method that gets messages from yahoo webmail event List Folder.
    /// Returns List of MailMsg.
    /// </summary>
    class YahooGetListedMessages : ISpotterVisitor
    {

        public static string GetAllInArray(XElement root, string xpath, string separator)
        {
            var sb = new StringBuilder();
            foreach (var element in root.XPathSelectElements(xpath))
            {
                sb.Append(element.Value + separator);
            }
            if (sb.Length > 0) sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }

        #region Implementation of ISpotterVisitor
        public object applyOn(SpotterJson spotter)
        {
            var root = spotter.GetRoot();
            var list = new List<MailMsg>();

            if(root.XPathSelectElement("/result//messageInfo") != null)
            {
                foreach (var conversation in root.XPathSelectElements("/result//messageInfo"))
                {
                    foreach(var msg in conversation.XPathSelectElements("item"))
                    {
                        var mail = new MailMsg();
                        mail.From = msg.XPathSelectElement("from/email").Value;
                        mail.To = msg.XPathSelectElement("toEmail").Value;
                        mail.Subject = msg.XPathSelectElement("subject").Value;
                        mail.Body = msg.XPathSelectElement("snippet").Value;
                        mail.SourceFolder = msg.XPathSelectElement("sourceFolderInfo/name").Value;
                        list.Add(mail);
                    }
                }
            }
            return list;
        }
        public object applyOn(SpotterKeyValue spotter) { throw new NotImplementedException(); }

        public object applyOn(SpotterMultipart spotter)
        {
            var list = new List<MailMsg>();

            spotter.Reset();
            while(spotter.HasNextPart())
            {
                var s = spotter.GetNextPart();
                var o = s.Accept(this);
                var partList = o as List<MailMsg>;
                if (partList != null) list.AddRange(partList);
                spotter.UnloadPart(s);
            }

            return list;
        }
        public object applyOn(SpotterText spotter) { throw new NotImplementedException(); }
        public object applyOn(SpotterFRPC spotter) { throw new NotImplementedException(); }
        #endregion
    }
}
