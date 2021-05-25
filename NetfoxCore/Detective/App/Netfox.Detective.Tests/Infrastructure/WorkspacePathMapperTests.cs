// Copyright (c) 2018 Hana Slamova
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
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Moq;
using Netfox.Core.Infrastructure;
using Netfox.Detective.Infrastructure;
using Netfox.Detective.Models.WorkspacesAndSessions;
using NUnit.Framework;

namespace Netfox.Detective.Tests.Infrastructure
{
    [TestFixture]
    class WorkspacePathMapperTests
    {
        private Mock<INetfoxSettings> _netfoxSettingsMock;

        [SetUp]
        public void SetUp() { this._netfoxSettingsMock = new Mock<INetfoxSettings>(); }

        [Test]
        public void Ctor_NetfoxSettingsIsNull_ThrowsArgumentNUllException()
        {
            // arrange & act & assert
            Assert.Throws<ArgumentNullException>(() => new WorkspacePathMapper(null));
        }

        [Test]
        public void FromWorkspace_WorkspacesIsNull_ThrowsArgumentNullException()
        {
            // arrange
            var sut  = new WorkspacePathMapper(this._netfoxSettingsMock.Object);

            // act & assert
            Assert.Throws<ArgumentNullException>(() => sut.FromWorkspace(null));
        }

        [Test]
        public void FromWorkspace_WorkspacesIsEmpty_ReturnsEmptyCollection()
        {
            // arrange
            var sut = new WorkspacePathMapper(this._netfoxSettingsMock.Object);

            // act
            var result = sut.FromWorkspace(new List<Workspace>());

            // assert
            Assert.IsEmpty(result);
        }

        [Test]
        public void FromWorkspace_WorkspacesIsNotEmpty_ReturnsNotEmptyCollection()
        {
            // arrange
            var sut = new WorkspacePathMapper(this._netfoxSettingsMock.Object);
            var item = new Workspace
            {
                WorkspaceDirectoryInfo = new DirectoryInfoWrapper(new MockFileSystem(),new DirectoryInfo("C:/someFakePath"))
            };

            // act
            var result = sut.FromWorkspace(new List<Workspace>{ item });

            // assert
            Assert.IsNotEmpty(result);
        }
    }
}
