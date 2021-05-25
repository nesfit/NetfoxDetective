using System.Windows.Controls;
using PostSharp.Patterns.Model;
using Telerik.Windows.Controls;

namespace Netfox.Detective.Core.BaseTypes.Views
{
    /// <summary>
    ///     Interaction logic for MainPane.xaml
    /// </summary>
    [NotifyPropertyChanged]
    public partial class DetectivePane : RadPane
    {
        public DetectivePane()
        {
            InitializeComponent();
        }
    }
}