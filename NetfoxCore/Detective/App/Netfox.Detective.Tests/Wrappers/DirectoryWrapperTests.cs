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
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Netfox.Detective.Wrappers;
using NUnit.Framework;


namespace Netfox.Detective.Tests.Wrappers
{
    [TestFixture]
    class DirectoryWrapperTests
    {
        private CancellationTokenSource _cancellationTokenSource;

        [SetUp]
        public void SetUp()
        {
            this._cancellationTokenSource = new CancellationTokenSource();
        }

        [TearDown]
        public void TearDown()
        {
           this._cancellationTokenSource.Cancel();
        }

       private static IEnumerable<TestCaseData> GetDeleteTestCases()
        {
            yield return new TestCaseData(true,false)
                .SetName("Delete_FilesLocked_WaitingForUnlock");
            yield return new TestCaseData(false,true)
                .SetName("Delete_FilesNotLocked_DeleteFiles");
        }

        [TestCaseSource(nameof(GetDeleteTestCases))]
        public void Delete(bool isFileLocked, bool deleteShouldBeexecuted)
        {
            // arrange
            var wrapper = new DirectoryWrapperMocked();
            wrapper.IsFileLocked = isFileLocked;

            // act
            var deleteTask = Task.Factory.StartNew(() => wrapper.Delete("someValidPathToDirectory"), this._cancellationTokenSource.Token);
            deleteTask.Wait(5000);

            // assert
            Assert.AreEqual(deleteShouldBeexecuted,wrapper.IsDeleteExecuted);
        }

        class DirectoryWrapperMocked: DirectoryWrapper
        {
            public bool IsDeleteExecuted { get; private set; } = false;
            protected override void Delete(string directoryFullName, bool isRecursive) { this.IsDeleteExecuted = true; }

            public bool IsFileLocked { get; set; } = true;
            protected override IDisposable OpenFile(string path, FileMode mode, FileAccess access, FileShare share)
            {
                if(IsFileLocked)
                {
                    throw new IOException();
                }
                else
                {
                    return Mock.Of<IDisposable>();
                }
            }

            protected override string[] GetFiles(string directoryFullName, string pattern, SearchOption option = SearchOption.AllDirectories)
            {
                return new[]
                {
                    "pathToLogFile1.log",
                    "pathToLogFile2.log"
                };
            }
        }
    }
}
