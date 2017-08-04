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

using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using Netfox.Detective.Core.BaseTypes.Views;

namespace Netfox.Detective.Views.FullTextSearch
{
    /// <summary>
    ///     Interaction logic for FullTextSearchView.xaml
    /// </summary>
    public partial class FullTextSearchView : PageViewBase
    {
        private const string DefaultName = "Fulltext search";
        private readonly FullTextSearchVm _context; //= new FullTextSearchVm();

        public FullTextSearchView()
        {
            this.InitializeComponent();
            this.ViewName = DefaultName;
            this.ControlVisible = Visibility.Collapsed;
            this.Visibility = Visibility.Hidden;

            this._context = (FullTextSearchVm) this.DataContext;
            //   DataContext = _context;
            this._context.NewFullTextSearchResult += this.context_NewFullTextSearchResult;
            this._context.PropertyChanged += this.context_PropertyChanged;
        }

        public static bool IsFocusable
        {
            get { return true; }
        }

        public bool CanSearchDb { get; set; }

        public IEnumerable<EncodingInfo> Encodings
        {
            get { return Encoding.GetEncodings(); }
        }

        public static DetectivePaneDockingPosition DetectivePaneDockingPosition
        {
            get { return DetectivePaneDockingPosition.Main; }
        }

        private void context_NewFullTextSearchResult(SearchResultVm searchResult)
        {
            //todo
            //var searchResultsView = new FullTextSearchResultsView(searchResult);
            //var searchResultsViewPane = new DetectivePane(searchResultsView);
            //MainSearchPanesGroup.Items.Add(searchResultsViewPane);
        }

        private void context_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "CanSearchDb")
            {
                this.CanSearchDb = this._context.CanSearchDb;
                this.OnPropertyChanged("CanSearchDb");
            }
        }
    }
}