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

using System;
using NUnit.Framework;

namespace Netfox.SnooperFacebook.Tests.Tests
{
    /// <summary>
    /// Test class for Facebook data mining. 
    /// Can't be tested by asserts, because most mined information is temporally (upcoming events, recent events, etc.)
    /// </summary>
    [TestFixture]
    [Explicit]
    [Category("Explicit")]
    class SnooperFacebookMiningTests
    {
        [Test]
        public void Data_Mining_public_info()
        {
            var miner = new SnooperFacebookMining(false);
            var publicInfo = miner.GetFacebookPublicInfo("bruckner.tomas");
            Assert.AreEqual(publicInfo.Gender, "male");
            Assert.AreEqual(publicInfo.Username, "bruckner.tomas");
            Assert.AreEqual(publicInfo.Id, 765374730);
        }

        [Test]
        public void Data_Mining_friends()
        {
            var miner = new SnooperFacebookMining(true);
            var friendlist = miner.GetFriendlist("vaclav.miculka");
            miner.QuitMining();
            Assert.GreaterOrEqual(friendlist.Friends.Count, 200);
        }

        [Test]
        public void Data_Mining_upcoming_events()
        {
            var miner = new SnooperFacebookMining(true);
            var fbEvents = miner.GetUpcomingEvents("socker01");
            miner.QuitMining();
            foreach(var fbEvent in fbEvents) {
                Console.WriteLine(fbEvent.Name);
            }
        }

        [Test]
        public void Data_Mining_past_events()
        {
            var miner = new SnooperFacebookMining(true);
            var fbEvents = miner.GetPastEvents("socker01");
            miner.QuitMining();
            miner.QuitMining();
            foreach (var fbEvent in fbEvents)
            {
                Console.WriteLine(fbEvent.Name);
            }
        }

        [Test]
        public void Data_Mining_albums()
        {
            var miner = new SnooperFacebookMining(true);
            var albums = miner.ParseAlbums("socker01");
            miner.QuitMining();
            foreach (var album in albums)
            {
                Console.WriteLine(album.Name);
            }
        }

        [Test]
        public void Data_Mining_recent_places()
        {
            var miner = new SnooperFacebookMining(true);
            var recentPlaces = miner.GetRecentPlaces("socker01");
            miner.QuitMining();
            Console.WriteLine(@"Test");
            foreach (var recentPlace in recentPlaces)
            {
                Console.WriteLine(recentPlace.Name);
            }
        }
    }
}
