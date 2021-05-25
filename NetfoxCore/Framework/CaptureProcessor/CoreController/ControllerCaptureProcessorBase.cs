using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

namespace Netfox.Framework.CaptureProcessor.CoreController
{
    internal abstract class ControllerCaptureProcessorBase
    {
        protected ControllerCaptureProcessorBase()
        {
        }

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

        public void ProcessCaptures(IEnumerable<FileInfo> captureFile)
        {
            this.ProcessCapturesInternal(captureFile);
        }

        public abstract void ProcessCapturesInternal(IEnumerable<FileInfo> captureFile);

        #region Exceptions

        [Serializable]
        public class UnknownFileType : Exception
        {
            public UnknownFileType(String message) : base(message)
            {
            }

            protected UnknownFileType(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
            {
            }
        }

        [Serializable]
        public class NotSuportedFileException : Exception
        {
            public NotSuportedFileException(String message) : base(message)
            {
            }

            protected NotSuportedFileException(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
            {
            }
        }

        #endregion
    }
}