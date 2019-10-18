using System.Collections.Generic;
using Netfox.Framework.Snoopers.SnooperWebmails.Models.WebmailEvents;

namespace Netfox.Framework.Snoopers.SnooperWebmails.Views
{
    /// <summary>
    /// Interaction logic for SnooperWebmailsView.xaml
    /// </summary>
    public partial class SnooperWebmailsView 
    {
        public SnooperWebmailsView()
        {
            this.InitializeComponent();
        }

        public void SetData(List<WebmailEventBase> messages)
        {
            this.WebmailEventsView.DataContext = messages;
        }
    }
}
