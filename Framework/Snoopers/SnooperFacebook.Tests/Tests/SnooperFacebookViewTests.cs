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

using System.Collections.Generic;
using Netfox.SnooperFacebook.Models;
using Netfox.SnooperFacebook.Models.Text;
using NUnit.Framework;

namespace Netfox.SnooperFacebook.Tests.Tests
{
    /// <summary>
    /// Proof of concept of Facebook views. THIS IS NOT ACTUAL UNIT TESTING.
    /// </summary>
    class SnooperFacebookViewTests
    {
        [Test][Ignore("Not implemented")]
        public void Facebook_Message_View_Test()
        {
            var test = new FacebookSnooperExport();
            var messages = new List<FacebookMessage>
            {
                new FacebookMessage(test)
                {
                    FbTimeStamp = 1000,
                    TargetId = 10515,
                    SenderId = 88,
                    Text = "ahoj"
                },
                new FacebookMessage(test)
                {
                    FbTimeStamp = 1000,
                    TargetId = 10515,
                    SenderId = 88,
                    Text = "ahoj2"
                },
                new FacebookMessage(test)
                {
                    FbTimeStamp = 1000,
                    TargetId = 10515,
                    SenderId = 88,
                    Text = "ahoj3"
                }
            };
           // FbMessagesView window;

           // The dispatcher thread
           //var t = new Thread(() =>
           //{
           //    window = new FbMessagesView();
           //    window.SetData(messages);
           //    window.FacebookMessagesView.DataContext = messages;
           //    Initiates the dispatcher thread shutdown when the window closes
           //     window.Closed += (s, e) => window.Dispatcher.InvokeShutdown();

           //    window.Show();

           //    Makes the thread support message pumping
           //     System.Windows.Threading.Dispatcher.Run();
           //});

           // Configure the thread
           // t.SetApartmentState(ApartmentState.STA);
           // t.Start();
           // t.Join();
        }
    }
}
