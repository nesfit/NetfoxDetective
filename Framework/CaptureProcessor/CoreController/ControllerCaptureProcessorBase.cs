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

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

namespace Netfox.Framework.CaptureProcessor.CoreController
{
    internal abstract class ControllerCaptureProcessorBase
    {
        protected ControllerCaptureProcessorBase() {}

        /// <summary>
        ///     this directory is used for saving capture l4 if enabled
        /// </summary>
        public DirectoryInfo WorkspaceDirectoryInfo { get; set; }

        public void ProcessCapture(FileInfo captureFile, Boolean createConversationsCrossCaptures = false)
        {
            this.ProcessCaptures(new List<FileInfo>
            {
                captureFile
            });
        }

        public void ProcessCaptures(IEnumerable<FileInfo> captureFile) { this.ProcessCapturesInternal(captureFile); }

        public abstract void ProcessCapturesInternal(IEnumerable<FileInfo> captureFile); 

        #region Exceptions
        [Serializable]
        public class UnknownFileType : Exception
        {
            public UnknownFileType(String message) : base(message) { }
            protected UnknownFileType(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt) { }
        }

        [Serializable]
        public class NotSuportedFileException : Exception
        {
            public NotSuportedFileException(String message) : base(message) { }
            protected NotSuportedFileException(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt) { }
        }
        #endregion
    }
}