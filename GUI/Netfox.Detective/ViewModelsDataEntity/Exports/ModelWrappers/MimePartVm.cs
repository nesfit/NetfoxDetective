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
using System.Collections.Generic;
using Netfox.Framework.Models.Snoopers.Email;

namespace Netfox.Detective.ViewModelsDataEntity.Exports.ModelWrappers
{
    public class MimePartVm
    {
        public MimePartVm(MIMEpart mimEpart) { this.MIMEpart = mimEpart; }

        public MIMEpart MIMEpart { get; private set; }

        public string Subject
        {
            get
            {
                if(this.MIMEpart == null) { return String.Empty; }

                return this.MIMEpart.Subject;
            }
        }

        public string From
        {
            get
            {
                if(this.MIMEpart == null) { return String.Empty; }

                return this.MIMEpart.From;
            }
        }

        public string To
        {
            get
            {
                if(this.MIMEpart == null) { return String.Empty; }

                return this.MIMEpart.To;
            }
        }

        public string FileName => this.MIMEpart.SuggestedFilename;

        public string FilePath
        {
            get { return this.MIMEpart.StoredContent.FullName; }
        }

        public IEnumerable<MimePartVm> ChildrenParts
        {
            get
            {
                if(this.MIMEpart.ContainedParts == null) { yield break; }

                foreach(var mimEpart in this.MIMEpart.ContainedParts) { yield return new MimePartVm(mimEpart); }
            }
        }

        public string RawHeaderAndContent => this.RawHeader + Environment.NewLine + this.RawContent;

        public string RawHeader => this.RawHeader;

        public string RawContent => this.MIMEpart.RawContent;
    }
}