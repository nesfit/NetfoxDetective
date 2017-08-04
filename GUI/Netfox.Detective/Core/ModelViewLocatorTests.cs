using Castle.Windsor;
using NUnit.Framework;

namespace Netfox.GUI.Detective.Core
{
    [TestFixture]
    internal class ModelViewLocatorTests
    {
        private dynamic _viewModelLocator;

        private ViewModelLocator ViewModelLocatorBase => this._viewModelLocator;

        private WindsorContainer WindsorContainer { get; set; }

        [Test]
        public void DynamicClassTest()
        {
            var BgTasksManagerVm = this._viewModelLocator.BgTasksManagerVm;
            Assert.IsTrue(BgTasksManagerVm != null);
            Assert.IsTrue(BgTasksManagerVm.IsEnabled == false);
        }

        [SetUp]
        public void SetUP()
        {
            this.WindsorContainer = new WindsorContainer();
            this._viewModelLocator = new ViewModelLocator();
            this.WindsorContainer.Install(this._viewModelLocator);
        }

        [TearDown]
        public void TearDown() => this._viewModelLocator = null;
    }
}