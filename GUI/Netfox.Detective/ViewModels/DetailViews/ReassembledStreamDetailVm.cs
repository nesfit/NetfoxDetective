using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Netfox.Detective.Core.BaseTypes.Views;
using Netfox.Detective.ViewModelsDataEntity.Conversations;

namespace Netfox.Detective.ViewModels.DetailViews
{
    class ReassembledStreamDetailVm : DetectiveViewModelBase
    {
        private DetectivePane _detailPane;
        private ConversationVm _conversation;
        public ReassembledStreamDetailVm(ConversationVm conversation)
        {
            _conversation = conversation;
            DataContext = _conversation;
        }


        public IEnumerable<EncodingInfo> Encodings
        {
            get { return Encoding.GetEncodings(); }
        }

        public DetectivePane DetailPane
        {
            set
            {
                _detailPane = value;

                _detailPane.Header = "Reassembled Stream";
                _detailPane.Visibility = Visibility.Visible;

                _detailPane.MinHeight = 300;
                _detailPane.MinWidth = 500;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            _conversation.SavePDUsAs(StreamListBox.SelectedItems.Cast<ReassembledStreamPduVm>());
        }

    }
}
