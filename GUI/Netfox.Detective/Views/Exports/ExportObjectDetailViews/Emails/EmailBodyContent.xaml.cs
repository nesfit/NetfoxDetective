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
using System.Windows;
using System.Windows.Controls;
using Netfox.Detective.ViewModelsDataEntity.Exports.ModelWrappers;
using Telerik.Windows.Documents.FormatProviders.Html;
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
            if(mimePartVm != null)
            {
                RadDocument content = null;
                var plain = true;
                if(!String.IsNullOrEmpty(mimePartVm.MIMEpart.ContentSubtype))
                {
                    if(mimePartVm.MIMEpart.ContentSubtype.Equals("html"))
                    {
                        content = new HtmlFormatProvider().Import(mimePartVm.RawContent);
                        plain = false;
                    }
                    else if(mimePartVm.MIMEpart.ContentSubtype.Equals("rtf"))
                    {
                        content = new RtfFormatProvider().Import(mimePartVm.RawContent);
                        plain = false;
                    }
                    /* else if (mimePartVm.Data.ContentSubtype.Equals("pdf"))
                     {
                         content = new PdfFormatProvider().Import(mimePartVm.RawContent);
                     }*/
                    else
                    {
                        content = new TxtFormatProvider().Import(mimePartVm.RawContent);
                        //content = new XamlFormatProvider().Import(mimePartVm.RawContent);
                    }
                }
                else
                {
                    // content = new XamlFormatProvider().Import(mimePartVm.RawContent);
                    //TxtFormatProvider formatProvider = new TxtFormatProvider();
                    content = new TxtFormatProvider().Import(mimePartVm.RawContent);
                    //content = new TxtDataProvider(mimePartVm.RawContent);
                }

                if(plain) { foreach(var span in content.EnumerateChildrenOfType<Span>()) { span.FontSize = 12; } }

                content.LineSpacing = 1.1;
                content.ParagraphDefaultSpacingAfter = 0;
                content.ParagraphDefaultSpacingBefore = 0;

                this.ContentTextBox.LineBreakingRuleLanguage = LineBreakingRuleLanguage.None;
                this.ContentTextBox.Document = content;
            }
        }
    }
}