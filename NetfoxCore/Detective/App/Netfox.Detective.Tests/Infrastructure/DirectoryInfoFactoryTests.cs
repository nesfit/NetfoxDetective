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
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Netfox.Detective.Infrastructure;
using NUnit.Framework;
using IDirectoryInfoFactory = Netfox.Detective.Interfaces.IDirectoryInfoFactory;

namespace Netfox.Detective.Tests.Infrastructure
{
    [TestFixture]
    public class DirectoryInfoFactoryTests
    {
        private MockFileSystem _mockedFileSystem;
        private IDirectoryInfoFactory _directoryInfoFactory;

        [SetUp]
        public void SetUp()
        {
            _mockedFileSystem = new MockFileSystem();
            _directoryInfoFactory = new DirectoryInfoFactory(_mockedFileSystem);
        }

        [Test]
        public void Ctor_ArgumentIsNull_ThrowsArgumentNullException()
        {
            // arrange & act & assert
            Assert.Throws<ArgumentNullException>(() => new DirectoryInfoFactory(null));
        }

        [Test]
        public void CreateWorkspaceDirectoryInfo_WorkspaceNameIsNull_ThrowsArgumentNullException()
        {
            // act & assert
            Assert.Throws<ArgumentNullException>(() => _directoryInfoFactory.CreateWorkspaceDirectoryInfo(null, "workspaceStoragePath"));
        }

        [Test]
        public void CreateWorkspaceDirectoryInfo_WorkspacesStoragePathIsNull_ThrowsArgumentNullException()
        {
            // act & assert
            Assert.Throws<ArgumentNullException>(() => _directoryInfoFactory.CreateWorkspaceDirectoryInfo("WorkspaceName", null));
        }

        [Test]
        public void CreateWorkspaceDirectoryInfo_Called_WorkspaceDirectorCreatedWithProperName()
        {
            // arrange
            var workspaceName = "someWorkspaceName";
            var workspacesStoragePath = "someWorkspaceStoragePath";

            // act
            var result = _directoryInfoFactory.CreateWorkspaceDirectoryInfo(workspaceName, workspacesStoragePath);
            
            // assert
            Assert.True(result.FullName.Contains(Path.Combine(workspacesStoragePath, workspaceName)));
        }

       
        [Test]
        public void CreateInvestigationsDirectoryInfo_WorkspaceDirectoryInfoIsNull_ThrowsArgumentNullException()
        {
            // arrange & act & assert
            Assert.Throws<ArgumentNullException>(() => this._directoryInfoFactory.CreateInvestigationsDirectoryInfo(null));

        }

        [Test]
        public void CreateInvestigationsDirectoryInfo_WorkspaceNameIsNull_ThrowsArgumentNullException()
        {
            // arrange & act & assert
            Assert.Throws<ArgumentNullException>(() => this._directoryInfoFactory.CreateInvestigationsDirectoryInfo(null,"someWorkspaceStoragePath"));

        }

        [Test]
        public void CreateInvestigationsDirectoryInfo_WorkspaceStoragePathIsNull_ThrowsArgumentNullException()
        {
            // arrange & act & assert
            Assert.Throws<ArgumentNullException>(() => this._directoryInfoFactory.CreateInvestigationsDirectoryInfo("WorskpaceName", null));

        }

        [Test]
        public void CreateInvestigationsDirectoryInfo_Called_Returns()
        {
            var workspaceName = "someWorkspaceName";
            var workspacesStoragePath = "workspacesStoragePath";

            var result = this._directoryInfoFactory.CreateInvestigationsDirectoryInfo(workspaceName, workspacesStoragePath);

            Assert.True(result.FullName.Contains(Path.Combine(workspacesStoragePath, workspaceName)));
        }

        [Test]
        public void CreateSubdirectoryInfo_DirectoryInfoIsNull_ThrowsArgumentNullException()
        {
            // arrange & act & assert
            Assert.Throws<ArgumentNullException>(() => this._directoryInfoFactory.CreateSubdirectoryInfo(null, "directoryName"));

        }

        [Test]
        public void CreateSubdirectoryInfo_DirectoryNameIsnUll_ThrowsArgumentNullException()
        {
            // arrange & act & assert
            Assert.Throws<ArgumentNullException>(() => this._directoryInfoFactory.CreateSubdirectoryInfo((DirectoryInfoBase)this._mockedFileSystem.DirectoryInfo.FromDirectoryName("someDirectoryName"), null));

        }

    }
}
