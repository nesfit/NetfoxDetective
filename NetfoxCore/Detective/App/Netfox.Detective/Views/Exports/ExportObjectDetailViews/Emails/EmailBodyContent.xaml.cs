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
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Netfox.Detective.ViewModelsDataEntity.Exports.ModelWrappers;
using Netfox.Framework.Models.Snoopers.Email;
using Telerik.Windows.Documents.FormatProviders.Html;
using Telerik.Windows.Documents.FormatProviders.Pdf;
using Telerik.Windows.Documents.FormatProviders.Rtf;
using Telerik.Windows.Documents.FormatProviders.Txt;
using Telerik.Windows.Documents.Model;

namespace Netfox.Detective.Views.Exports.ExportObjectDetailViews.Emails
{
    /// <summary>
    ///     Interaction logic for EmailBodyContent.xaml
    /// </summary>
    public partial class EmailBodyContent : UserControl
    {
        public EmailBodyContent()
        {
            this.InitializeComponent();
            this.DataContextChanged += this.EmailBodyContent_DataContextChanged;
        }

        void EmailBodyContent_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var mimePartVm = e.NewValue as MimePartVm;
            if (mimePartVm != null)
            {
                var radDoc = CreateFlowDoc(mimePartVm.MIMEpart, mimePartVm.RawContent);

                // DONE: RadDocument (.NET 4.5) incompatible with RadFlowDocument (.NET Core)
                if (IsPlain(mimePartVm.MIMEpart))
                    foreach (var span in radDoc.EnumerateChildrenOfType<Span>())
                        span.FontSize = 12;

                radDoc.LineSpacing = 1.1;
                radDoc.ParagraphDefaultSpacingAfter = 0;
                radDoc.ParagraphDefaultSpacingBefore = 0;

                this.ContentTextBox.LineBreakingRuleLanguage = LineBreakingRuleLanguage.None;

                this.ContentTextBox.Document = radDoc;
            }
        }

        private static RadDocument CreateFlowDoc(MIMEpart mime, string data)
        {
            return mime.ContentSubtype switch
            {
                "html" => new HtmlFormatProvider().Import(data),
                "rtf" => new RtfFormatProvider().Import(data),
                "pdf" => new PdfFormatProvider().Import(Encoding.ASCII.GetBytes(data)),
                _ => new TxtFormatProvider().Import(data)
            };
        }

        private static bool IsPlain(MIMEpart mime) => mime.ContentSubtype is not "html" or "rtf" or "pdf";
    }
}