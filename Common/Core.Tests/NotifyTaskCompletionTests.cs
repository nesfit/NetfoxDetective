// Copyright (c) 2017 Jan Pluskal
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

using System.Threading;
using System.Threading.Tasks;
using Netfox.Core.Helpers;
using NUnit.Framework;

namespace Netfox.Core.Tests
{
    [TestFixture]
    public class NotifyTaskCompletionTests
    {
        [Test]
        public async Task Test()
        {
            var notifyTask = new NotifyTaskCompletion<bool>(async () => await Task.Run(()=>true));
            await notifyTask;
            Assert.IsTrue(notifyTask);
        }

        [Test]
        public void Test1()
        {
            var notifyTask = new NotifyTaskCompletion<bool>(async () => await Task.Run(async () =>
             {
                 await Task.Delay(100);
                 return true;
             }), false);
            Assert.IsFalse(notifyTask);
        }

        [Test]
        public async Task TaskAwait()
        {
            var notifyTask = new NotifyTaskCompletion<bool>(async () => await Task.Run(async () =>
            {
                await Task.Delay(100);
                return true;
            }), false);
            Assert.IsFalse(notifyTask);
            await notifyTask;
            Assert.IsTrue(notifyTask);
        }

        [Test]
        public async Task DoubleAwaitTest()
        {
            var notifyTask = new NotifyTaskCompletion<bool>(async () => await Task.Run(async () =>
            {
                await Task.Delay(100);
                return true;
            }), false);

            await notifyTask;
            await notifyTask;
            Assert.IsTrue(notifyTask);
        }

        [Test]
        public void TaskAwaitInConstructor()
        {
            var notifyTask = new NotifyTaskCompletion<bool>(async () => await Task.Run(async () =>
            {
                await Task.Delay(100);
                return true;
            }), true);
            Assert.IsFalse(notifyTask);
            var resetEvent = new ManualResetEvent(false);
            notifyTask.PropertyChanged += (sender, args) => resetEvent.Set();
            resetEvent.WaitOne();
            Assert.IsTrue(notifyTask);
        }
    }
}
