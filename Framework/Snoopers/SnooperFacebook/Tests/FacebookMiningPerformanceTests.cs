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
using System.Diagnostics;
using Netfox.SnooperFacebook.ModelsMining.Person;
using NUnit.Framework;

namespace Netfox.SnooperFacebook.Tests
{
    [TestFixture]
    [Explicit]
    [Category("Explicit")]
    class FacebookMiningPerformanceTests
    {
        [Test]
        public void Data_Mining_public_info()
        {
            var watch = new Stopwatch();
            // clean up
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            watch.Start();
            var miner = new SnooperFacebookMining(false);
            for(var i = 0; i < 5; i++)
            {
                miner.GetFacebookPublicInfo("bruckner.tomas");
            }

            watch.Stop();

            Debug.WriteLine(@"Benchmark public info: {0} ms", watch.Elapsed.TotalMilliseconds);

        }

        [Test]
        public void Data_Mining_friendlist()
        {
            var watch = new Stopwatch();
            // clean up
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            watch.Start();
            var miner = new SnooperFacebookMining(true);

            for(var i = 0; i < 1; i++) { miner.GetFriendlist("vaclav.miculka"); }
            miner.QuitMining();


            watch.Stop();

            Debug.WriteLine(@"Benchmark friendlist: {0} ms", watch.Elapsed.TotalMilliseconds);
        }

        [Test]
        public void Data_Mining_recent_places()
        {
            var watch = new Stopwatch();
            // clean up
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            watch.Start();
            var miner = new SnooperFacebookMining(true);

            for(var i = 0; i < 10; i++) { miner.GetRecentPlaces("socker1"); }
            miner.QuitMining();

            watch.Stop();

            Debug.WriteLine(@"Benchmark recent places: {0} ms", watch.Elapsed.TotalMilliseconds);
        }

        [Test]
        public void Data_Mining_upcomming_events()
        {
            var watch = new Stopwatch();
            // clean up
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            watch.Start();
            var miner = new SnooperFacebookMining(true);
            for(var i = 0; i < 10; i++) { miner.GetUpcomingEvents("socker01"); }
            miner.QuitMining();

            watch.Stop();

            Debug.WriteLine(@"Benchmark upcoming events: {0} ms", watch.Elapsed.TotalMilliseconds);
        }

        [Test]
        public void Data_Mining_past_events()
        {
            var watch = new Stopwatch();
            // clean up
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            watch.Start();
            var miner = new SnooperFacebookMining(true);
            for(var i = 0; i < 1; i++) { miner.GetPastEvents("socker01"); }
            miner.QuitMining();

            watch.Stop();

            Debug.WriteLine(@"Benchmark past events: {0} ms", watch.Elapsed.TotalMilliseconds);
        }

        [Test]
        public void Data_Mining_albums()
        {
            var watch = new Stopwatch();
            // clean up
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            watch.Start();

            var miner = new SnooperFacebookMining(true);
            for(var i = 0; i < 10; i++) { miner.ParseAlbums("vaclav.miculka"); }
            miner.QuitMining();

            watch.Stop();

            Debug.WriteLine(@"Benchmark albums: {0} ms", watch.Elapsed.TotalMilliseconds);
        }

        [Test]
        public void Data_Mining_mutual()
        {
            var watch = new Stopwatch();
            // clean up
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();


            var friendlist1 = new FacebookFriendlist
            {
                Username = "username1"
            };
            friendlist1.Friends.Add("friend1");
            friendlist1.Friends.Add("friend2");
            friendlist1.Friends.Add("friend3");
            friendlist1.Friends.Add("friend4");
            friendlist1.Friends.Add("friend5");
            friendlist1.Friends.Add("friend6");
            friendlist1.Friends.Add("friend7");
            friendlist1.Friends.Add("friend8");
            friendlist1.Friends.Add("friend9");
            friendlist1.Friends.Add("friend10");
            friendlist1.Friends.Add("friend11");
            friendlist1.Friends.Add("friend12");
            friendlist1.Friends.Add("friend13");
            friendlist1.Friends.Add("friend14");
            friendlist1.Friends.Add("friend15");
            friendlist1.Friends.Add("friend16");
            friendlist1.Friends.Add("friend17");
            friendlist1.Friends.Add("friend18");
            friendlist1.Friends.Add("friend19");
            friendlist1.Friends.Add("friend111");
            friendlist1.Friends.Add("friend112");
            friendlist1.Friends.Add("friend113");
            friendlist1.Friends.Add("friend114");
            friendlist1.Friends.Add("friend115");

            var friendlist2 = new FacebookFriendlist
            {
                Username = "username2"
            };
            friendlist1.Friends.Add("friend1");
            friendlist1.Friends.Add("xfriend2");
            friendlist1.Friends.Add("friend3");
            friendlist1.Friends.Add("xfriend4");
            friendlist1.Friends.Add("friend5");
            friendlist1.Friends.Add("xfriend6");
            friendlist1.Friends.Add("friend7");
            friendlist1.Friends.Add("xfriend8");
            friendlist1.Friends.Add("friend9");
            friendlist1.Friends.Add("xfriend10");
            friendlist1.Friends.Add("friend11");
            friendlist1.Friends.Add("xfriend12");
            friendlist1.Friends.Add("friend13");
            friendlist1.Friends.Add("xfriend14");
            friendlist1.Friends.Add("friend15");
            friendlist1.Friends.Add("xfriend16");
            friendlist1.Friends.Add("friend17");
            friendlist1.Friends.Add("xfriend18");
            friendlist1.Friends.Add("friend19");
            friendlist1.Friends.Add("xfriend111");
            friendlist1.Friends.Add("friend112");
            friendlist1.Friends.Add("xfriend113");
            friendlist1.Friends.Add("friend114");
            friendlist1.Friends.Add("xfriend115");
            watch.Start();

            var miner = new SnooperFacebookMining(true);
            for (var i = 0; i < 10; i++) { miner.GetMutualFriends(friendlist1, friendlist2); }
            miner.QuitMining();

            watch.Stop();

            Debug.WriteLine(@"Benchmark mutual: {0} ms", watch.Elapsed.TotalMilliseconds);
        }
    }
}
