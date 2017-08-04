// The MIT License (MIT)
//  
// Copyright (c) 2012-2013 Brno University of Technology - Faculty of Information Technology (http://www.fit.vutbr.cz)
// Author(s):
// Martin Mares (mailto:xmares04@stud.fit.vutbr.cz)
//  
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify,
// merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 

using System;
using System.Collections.Generic;
using System.Linq;
using Netfox.Detective.ViewModelsDataEntity.Exports;

namespace Netfox.Detective.Views.Exports.Explorers
{
    /// <summary>
    ///     Interaction logic for WebExplorer.xaml
    /// </summary>
    public partial class WebExplorer : ExportViewBase, IExportViewExplorer
    {
        private ExportResultVm _exportResultContext;

        public WebExplorer()
        {
            this.InitializeComponent();

            this.ViewName = "Web";
            this.viewType = ViewType.ExportResultExplorer;
        }

        public ExportDataVm ExportDataContext { get; set; }

        public ExportResultVm ExportResultContext
        {
            get { return this._exportResultContext; }
            set
            {
                var visible = false;

                var emailExporter = new List<Type>(HttpExport.SupportedExporters);
                if(emailExporter.Contains(value.ExportResult.ExporterType))
                {
                    this._exportResultContext = value;
                    this.DataContext = this._exportResultContext;

                    if(this._exportResultContext != null)
                    {
                        visible = true;
                        //BringToFrontExportViewMessage.SendBringToFrontMessage(this);
                        this.OnViewAvialable(this, ViewAvialableQuality.Good);
                    }
                }

                this.IsVisible(visible);


                if(visible)
                {
                    var first = this._exportResultContext.ExportData.FirstOrDefault();
                    //_exportResultContext.SelectedData = first;
                    this._exportResultContext.SelectData(first, false);
                }
            }
        }
    }
}