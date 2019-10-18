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

namespace Netfox.Core.Interfaces.Model.Exports
{    /// <summary> Distinguishes between different email types behind common interface.</summary>
    public enum EMailType
    {
        /// <summary> An enum constant representing the unknown option.</summary>
        Unknown,
        /// <summary> An enum constant representing the raw email option.</summary>
        RawEmail,
        /// <summary> An enum constant representing the pop 3 organisation email option.</summary>
        POP3OrgEmail,
        /// <summary> An enum constant representing the SMTP organisation email option.</summary>
        SMTPOrgEmail,
        /// <summary> An enum constant representing the IMAP organisation email option.</summary>
        IMAPOrgEmail,
        //  Crypto
    }

    /// <summary> Values that represent EMailContentType.</summary>
    public enum EMailContentType
    {
        /// <summary> An enum constant representing the unknown option.</summary>
        Unknown,
        /// <summary> An enum constant representing the whole option.</summary>
        Whole,
        /// <summary> An enum constant representing the fragment option.</summary>
        Fragment,
        /// <summary> An enum constant representing the crypto option.</summary>
        Crypto
    }
    /// <summary> Common email interface - ensures basic functionality.</summary>
    public interface IEMail : IExportBase
    {
        /// <summary> Type of email behind this interface.</summary>
        /// <value> The type.</value>
        EMailType EMailType { get; set; }

        /// <summary> Gets or sets the type of the content.</summary>
        /// <value> The type of the content.</value>
        EMailContentType ContentType { get; set; }

        string From { get; }
        string To { get; }
        string Bcc { get; }
        string Cc { get; }
        string Subject { get; }
        string RawContent { get; }
    }
}