using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Netfox.Detective.Core.BaseTypes.Views;
using Netfox.Detective.ViewModelsDataEntity.Conversations;

namespace Netfox.Detective.Views.DetailViews
{
    /// <summary>
    ///     Interaction logic for TaxonomyDetail.xaml
    /// </summary>
    public partial class TaxonomyDetail : UserControl, IDetailView, INotifyPropertyChanged
    {
        private DetectivePane _detailPane;
        private TaxonomyProtocol _selectedTaxonomyProtocol;
        private List<TaxonomyCategory> _taxonomies;

        public TaxonomyDetail(ConversationVm conversation)
        {
            this.InitializeComponent();
            //_conversation = conversation;
            this.DataContext = this;

            this.GenerateTaxonomyTree(conversation.Conversation.FwControllerContext, conversation);
            this.OnPropertyChanged("CategoryTaxonomies");
            this.OnPropertyChanged("SelectedTaxonomyProtocol");
        }

        public TaxonomyDetail(FwControllerContext fwControllerContext)
        {
            this.InitializeComponent();
            this.DataContext = this;

            this.GenerateTaxonomyTree(fwControllerContext);
            this.OnPropertyChanged("CategoryTaxonomies");
            this.OnPropertyChanged("SelectedTaxonomyProtocol");
        }

        //private ConversationVm _conversation;
        public TaxonomyProtocol SelectedTaxonomyProtocol
        {
            get { return this._selectedTaxonomyProtocol; }
            private set
            {
                this._selectedTaxonomyProtocol = value;
                this.OnPropertyChanged("SelectedTaxonomyProtocol");
            }
        }

        public IEnumerable<EncodingInfo> Encodings
        {
            get { return Encoding.GetEncodings(); }
        }

        public IEnumerable<TaxonomyCategory> CategoryTaxonomies
        {
            get { return this._taxonomies; }
        }

        public DetectivePane DetailPane
        {
            set
            {
                this._detailPane = value;

                this._detailPane.Header = "Conversation taxonomy";
                this._detailPane.Visibility = Visibility.Visible;

                this._detailPane.Width = 700;
                this._detailPane.Height = 400;

                //_detailPane.FloatingSize = "700 400";
                this._detailPane.MakeFloatingDockable();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void GenerateTaxonomyTree(FwControllerContext fwControllerContext, ConversationVm conversation = null)
        {
            this._taxonomies = new List<TaxonomyCategory>();


            NBAR2TaxonomyProtocol[] nbarTaxonomies;
            if(conversation == null)
            {
                var nbarDb = fwControllerContext.FwController.GetNbarProtocolPortDatabase();
                nbarTaxonomies = nbarDb.Nbar2Taxonomy.NBAR2TaxonomyProtocol;
            }
            else
            { nbarTaxonomies = fwControllerContext.FwController.GetNbarTaxonomiesProtocolFromAppTags(conversation.Conversation.ApplicationTags); }


            foreach(var nbarTaxonomy in nbarTaxonomies)
            {
                var attributes = nbarTaxonomy.attributes.FirstOrDefault();
                if(attributes != null)
                {
                    var category = this._taxonomies.FirstOrDefault(taxonomyCategory => taxonomyCategory.Name == attributes.category);

                    if(category == null)
                    {
                        category = new TaxonomyCategory
                        {
                            Name = attributes.category,
                            Protocols = new List<TaxonomyProtocol>(),
                            SubCategories = new List<TaxonomyCategory>()
                        };

                        this._taxonomies.Add(category);
                    }

                    var subCategory = category.SubCategories.FirstOrDefault(taxonomyCategory => taxonomyCategory.Name == attributes.subcategory);

                    if(subCategory == null)
                    {
                        subCategory = new TaxonomyCategory
                        {
                            Name = attributes.subcategory,
                            Protocols = new List<TaxonomyProtocol>(),
                            SubCategories = new List<TaxonomyCategory>()
                        };

                        category.SubCategories.Add(subCategory);
                    }

                    var applicationGroup = subCategory.SubCategories.FirstOrDefault(taxonomyCategory => taxonomyCategory.Name == attributes.applicationgroup);

                    if(applicationGroup == null)
                    {
                        applicationGroup = new TaxonomyCategory
                        {
                            Name = attributes.applicationgroup,
                            Protocols = new List<TaxonomyProtocol>(),
                            SubCategories = new List<TaxonomyCategory>()
                        };

                        subCategory.SubCategories.Add(applicationGroup);
                    }

                    var taxProto = new TaxonomyProtocol
                    {
                        Name = nbarTaxonomy.name,
                        CommonName = nbarTaxonomy.commonname,
                        Description = nbarTaxonomy.helpstring,
                        Encrypted = attributes.encrypted,
                        Tunnel = attributes.tunnel,
                        UnderLyingProtocols = nbarTaxonomy.underlyingprotocols
                    };


                    foreach(var port in nbarTaxonomy.ports)
                    {
                        if(!string.IsNullOrEmpty(port.tcp)) { taxProto.Ports += string.Format("TCP:{0} ", port.tcp); }

                        if(!string.IsNullOrEmpty(port.udp)) { taxProto.Ports += string.Format("UDP:{0} ", port.udp); }
                    }

                    applicationGroup.Protocols.Add(taxProto);
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = this.PropertyChanged;
            if(handler != null) { handler(this, new PropertyChangedEventArgs(propertyName)); }
        }

        private void TreeView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = this.TaxonomiesTreeView.SelectedItem;
            if(selected != null)
            {
                var type = selected.GetType();
                if(type == typeof(TaxonomyProtocol))
                {
                    this.ProtocolDetail.DataContext = selected;
                    this.SelectedTaxonomyProtocol = selected as TaxonomyProtocol;
                }
            }
        }

        public interface ITaxonomyItem
        {
            string Name { get; set; }
        }

        public class TaxonomyCategory : ITaxonomyItem
        {
            public List<TaxonomyCategory> SubCategories { get; set; }
            public List<TaxonomyProtocol> Protocols { get; set; }

            public IEnumerable<ITaxonomyItem> Items
            {
                get
                {
                    if(this.SubCategories != null) { foreach(var taxonomyCategory in this.SubCategories) { yield return taxonomyCategory; } }

                    if(this.Protocols != null) { foreach(var taxonomyProtocol in this.Protocols) { yield return taxonomyProtocol; } }
                }
            }

            public string Name { get; set; }
        }

        public class TaxonomyProtocol : ITaxonomyItem
        {
            public string CommonName { get; set; }
            public string Description { get; set; }
            public string Ports { get; set; }
            public string UnderLyingProtocols { get; set; }
            public string Tunnel { get; set; }
            public string Encrypted { get; set; }
            public string Name { get; set; }
        }
    }
}