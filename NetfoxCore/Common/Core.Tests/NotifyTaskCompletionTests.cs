using System.Threading;
using System.Threading.Tasks;
using Netfox.Core.Helpers;
using NUnit.Framework;

namespace Core.Tests
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