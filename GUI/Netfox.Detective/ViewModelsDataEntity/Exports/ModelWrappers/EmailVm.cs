// Copyright (c) 2017 Jan Pluskal, Martin Mares, Martin Kmet
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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Castle.Windsor;
using Netfox.Core.Enums;
using Netfox.Core.Interfaces.Model.Exports;
using Netfox.Framework.Models.Snoopers.Email;

namespace Netfox.Detective.ViewModelsDataEntity.Exports.ModelWrappers
{
    public class EmailVm : DetectiveDataEntityViewModelBase
    {
        private MimePartVm _mimeEmail; // TODO - ??????????
        private bool _rootInitialized;
        private MimePartVm _selectedPart;
        public EmailVm(WindsorContainer applicationWindsorContainer) : base(applicationWindsorContainer) { }

        public EmailVm(WindsorContainer applicationWindsorContainer, object model, ExportVm exportVm) : base(applicationWindsorContainer, model)
        {
            this.Email = model as IEMail;
            this.ExportVm = exportVm;
        }

        public IEMail Email { get;  }
        public ExportVm ExportVm { get; }

        public string EventType
        {
            get
            {
                switch(this.ExportVm.Export.ExportValidity)
                {
                    case ExportValidity.ValidWhole:
                        return "Email";
                    case ExportValidity.ValidFragment:
                        return "Headers";
                    default:
                        return string.Empty;
                }
            }
        }

        public DateTime TimeStamp
        {
            get
            {
                if(this.EmailRoot == null) { return DateTime.MinValue; }
                DateTime result;

                if(DateTime.TryParseExact(this.EmailRoot.MIMEpart.Date, "yyyy-MM-dd hh.mm.ss.fffffff", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                {
                    result = DateTime.SpecifyKind(result, DateTimeKind.Utc);
                    return result;
                }

                return this.Email.TimeStamp;
            }
        }

        public string From
        {
            get
            {
                if(this.EmailRoot == null) { return String.Empty; }

                return this.EmailRoot.From;
            }
        }

        public string To
        {
            get
            {
                if(this.EmailRoot == null) { return String.Empty; }

                return this.EmailRoot.To;
            }
        }

        public string[] ToList
        {
            get
            {
                if(this.EmailRoot == null) { return new string[0]; }

                return this.EmailRoot.To.Split(',');
            }
        }

        public string Subject
        {
            get
            {
                if(this.EmailRoot == null) { return String.Empty; }

                return this.EmailRoot.Subject;
            }
        }

        public bool HasAttachment => this.Attachments.Any();

        public string Description
        {
            get
            {
                if(this.Email == null) { return String.Empty; }

                var defaultDescription = $"{this.Email.SourceEndPoint} > {this.Email.DestinationEndPoint}";

                if(this.Email.EMailType == EMailType.RawEmail) { return defaultDescription; }

                var mimeEmail = (MIMEemail) this.Email;
                if(mimeEmail.DocumentRoot == null) { return defaultDescription; }

                var from = this.From;
                var to = this.To;

                if(String.IsNullOrEmpty(from) && this.Email.SourceEndPoint != null) { from = this.Email.SourceEndPoint.ToString(); }

                if(String.IsNullOrEmpty(to) && this.Email.DestinationEndPoint != null) { to = this.Email.DestinationEndPoint.ToString(); }

                return $"{@from}\r\n{to}";
            }
        }

        public string ShortPlainBody
        {
            get
            {
                var shortPlainBody = String.Empty;
                var body = this.Bodies.FirstOrDefault(b => b.MIMEpart.ContentSubtype.Equals("plain"));
                if(body == null)
                {
                    body = this.Bodies.FirstOrDefault(b => b.MIMEpart.ContentSubtype.Equals("html"));
                    if(body == null) { return String.Empty; }
                    shortPlainBody = StripHtml(body.RawContent);
                    if(shortPlainBody.Length > 150) { shortPlainBody = shortPlainBody.Substring(0, 150) + " ..."; }
                }
                else
                {
                    if(body.RawContent.Length > 150) {
                        shortPlainBody = body.RawContent.Substring(0, 150) + " ...";
                    }
                    else
                    {
                        shortPlainBody = body.RawContent;
                    }
                }

                shortPlainBody = Regex.Replace(shortPlainBody, @"\s+", " ");
                shortPlainBody = shortPlainBody.Replace("\r", "");
                shortPlainBody = shortPlainBody.Replace("\n", " ");

                return shortPlainBody;
            }
        }

        public long RawDataSize => this.Email.RawContent.Length;

        public MimePartVm SelectedPart
        {
            get { return this._selectedPart; }
            set
            {
                this._selectedPart = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.SelectedPartContent));
                this.OnPropertyChanged(nameof(this.SelectedPartContentHeader));
                this.OnPropertyChanged(nameof(this.SelectedPartHeader));
            }
        }

        public string SelectedPartContentHeader => this.SelectedPartHeader + Environment.NewLine + this.SelectedPartContent;

        public string SelectedPartHeader
        {
            get
            {
                if(this.SelectedPart == null) { return String.Empty; }

                return this.SelectedPart.RawHeader;
            }
        }

        public string SelectedPartContent
        {
            get
            {
                if(this.SelectedPart == null) { return String.Empty; }

                return this.SelectedPart.RawContent;
            }
        }
        

        public MimePartVm EmailRoot
        {
            get
            {
                /* if (Data.Email == null || Data.Email.Type == EmailExport.EmailType.RawEmail)
                     return null;

                 EmailExport.MIMEemail mimeEmail = (EmailExport.MIMEemail) Data.Email;
                 MimePartVm mimeModel = new MimePartVm(mimeEmail.MimeRoot,InvestigationWorkspace);
                 return mimeModel;*/

                if(!this._rootInitialized)
                {
                    if(this.Email != null && this.Email.EMailType != EMailType.RawEmail)
                    {
                        var mimeEmailData = (MIMEemail) this.Email;
                        this._mimeEmail = new MimePartVm(mimeEmailData.DocumentRoot);
                    }
                    this._rootInitialized = true;
                }

                return this._mimeEmail;
            }
        }

        public IEnumerable EmailTree
        {
            get { yield return this.EmailRoot; }
        }

        public IEnumerable<MimePartVm> FlatEmail
        {
            get
            {
                var flatpartsList = this.GetFlatParts(this.EmailRoot).ToList();
                Console.WriteLine(flatpartsList.Count());
                return this.GetFlatParts(this.EmailRoot);
            }
        }

        public IEnumerable<MimePartVm> Bodies
        {
            get
            {
                foreach(var emailPart in this.FlatEmail)
                {
                    if(!String.IsNullOrEmpty(emailPart.MIMEpart.ContentType) && !String.IsNullOrEmpty(emailPart.MIMEpart.ContentSubtype)
                       && (emailPart.MIMEpart.ContentType.Equals("text") && (emailPart.MIMEpart.ContentSubtype.Equals("plain") || emailPart.MIMEpart.ContentSubtype.Equals("html"))))
                    {
                        //if (emailPart.Data.ContentSubtype.Equals("html"))
                        yield return emailPart;
                    }
                }
            }
        }

        public IEnumerable<MimePartVm> Attachments
        {
            get
            {
                foreach(var emailPart in this.FlatEmail)
                {
                    if(!String.IsNullOrEmpty(emailPart.MIMEpart.ContentDisposition)
                       && (emailPart.MIMEpart.ContentDisposition.Equals("attachment") || emailPart.MIMEpart.ContentDisposition.Equals("inline"))) {
                           yield return emailPart;
                       }
                }
            }
        }

        public IEnumerable<MimePartVm> GetFlatParts(MimePartVm mimePart)
        {
            if(mimePart != null)
            {
                yield return mimePart;
                foreach(var part in mimePart.ChildrenParts)
                {
                    //   yield return part;
                    var subParts = this.GetFlatParts(part);
                    foreach(var subPart in subParts) { yield return subPart; }
                }
            }
        }

        private static string StripHtml(string source)
        {
            //get rid of HTML tags
            var output = Regex.Replace(source, "<[^>]*>", string.Empty);

            //get rid of multiple blank lines
            output = Regex.Replace(output, @"^\s*$\n", string.Empty, RegexOptions.Multiline);

            return output;
        }
    }
}