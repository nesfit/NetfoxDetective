// Copyright (c) 2017 Jan Pluskal, Tomas Bruckner
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

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Netfox.Core.Properties;
using Netfox.SnooperFacebook.ModelsMining.Events;
using Netfox.SnooperFacebook.ModelsMining.Person;
using Netfox.SnooperFacebook.ModelsMining.Photos;
using Netfox.SnooperFacebook.ModelsMining.Places;
using Telerik.Windows.Data;

namespace Netfox.SnooperFacebook.WPF.ViewModels
{
    public class FacebookMiningViewModel : INotifyPropertyChanged
    {
        public RadObservableCollection<FacebookAlbum> Albums { get; set; }
        public FacebookFriendlist Friendlist { get; set; }
        public RadObservableCollection<FacebookUpcomingEvent> UpcomingEvents { get; set; }
        public RadObservableCollection<FacebookPastEvent> PastEvents { get; set; }
        public RadObservableCollection<FacebookPublicInfo> PublicInfos { get; set; }
        public RadObservableCollection<FacebookRecentPlace> RecentPlaces { get; set; }

        public FacebookMiningViewModel()
        {
            this.InitCollections();
        }

        private void InitCollections()
        {
            this.Albums = new RadObservableCollection<FacebookAlbum>();
            this.UpcomingEvents = new RadObservableCollection<FacebookUpcomingEvent>();
            this.PastEvents = new RadObservableCollection<FacebookPastEvent>();
            this.PublicInfos = new RadObservableCollection<FacebookPublicInfo>();
            this.RecentPlaces = new RadObservableCollection<FacebookRecentPlace>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    }
}
