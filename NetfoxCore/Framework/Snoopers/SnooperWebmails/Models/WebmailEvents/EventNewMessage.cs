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

using System.ComponentModel.DataAnnotations.Schema;
using Netfox.Core.Interfaces.Model.Exports;
using Netfox.Framework.Models.Snoopers;

namespace Netfox.Snoopers.SnooperWebmails.Models.WebmailEvents
{
    /// <summary>
    ///     New Mail event.
    /// </summary>
    public class EventNewMessage : WebmailEventBase, IEMail
    {
        public MailMsg Mail { get; private set; }
        private EventNewMessage() : base() { } //EF

        public EventNewMessage(SnooperExportBase exportBase, MailMsg m) : base(exportBase) { this.Mail = m; }

        public EventNewMessage(SnooperExportBase exportBase, string from, string to, string cc, string bcc, string subject, string content) : base(exportBase)
        {
            this.Mail = new MailMsg
            {
                From = @from,
                To = to,
                Cc = cc,
                Bcc = bcc,
                Subject = subject,
                Body = content
            };
        }

        [NotMapped]
        public string Bcc => this.Mail.Bcc;
        [NotMapped]
        public string Cc => this.Mail.Cc;

        /// <summary> Gets or sets the type of the content.</summary>
        /// <value> The type of the content.</value>
        public EMailContentType ContentType { get; set; } = EMailContentType.Unknown;

        //public EventNewMessage(SnooperExportBase exportBase, object mail) : base(exportBase)
        //{
        //    this.Mail = new MailMsg();

        //    var emailFields = mail as Dictionary<string, List<string>>;
        //    if (emailFields != null)
        //    {
        //        this.Mail.From = emailFields.ContainsKey("from")? emailFields["from"].First() : string.Empty;
        //        this.Mail.To = emailFields.ContainsKey("to")? emailFields["to"].First() : string.Empty;
        //        this.Mail.Cc = emailFields.ContainsKey("cc")? emailFields["cc"].First() : string.Empty;
        //        this.Mail.Bcc = emailFields.ContainsKey("bcc")? emailFields["bcc"].First() : string.Empty;
        //        this.Mail.Subject = emailFields.ContainsKey("subject")? emailFields["subject"].First() : string.Empty;
        //        this.Mail.Body = emailFields.ContainsKey("body")? emailFields["body"].First() : emailFields.ContainsKey("content")? emailFields["content"].First() : string.Empty;
        //    }
        //    var mailContent = mail as XElement;
        //    if(mailContent != null)
        //    {
        //        this.Mail.From = mailContent.XPathSelectElement("//from").Value;
        //        this.Mail.To = mailContent.XPathSelectElement("//to").Value;
        //        this.Mail.Cc = mailContent.XPathSelectElement("//cc").Value;
        //        this.Mail.Bcc = mailContent.XPathSelectElement("//bcc").Value;
        //        this.Mail.Subject = mailContent.XPathSelectElement("//subject").Value;
        //        this.Mail.Body = mailContent.XPathSelectElement("//simpleBody").Value;
        //    }

        //}

        /// <summary> Type of email behind this interface.</summary>
        /// <value> The type.</value>
        public EMailType EMailType { get; set; } = EMailType.Unknown;
        [NotMapped]
        public string From => this.Mail.From;
        [NotMapped]
        public string RawContent => this.Mail.Body;
        [NotMapped]
        public string Subject => this.Mail.Subject;
        [NotMapped]
        public string To => this.Mail.To;
    }
}