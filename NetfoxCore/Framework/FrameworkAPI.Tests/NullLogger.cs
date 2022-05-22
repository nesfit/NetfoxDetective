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
using Castle.Core.Logging;

namespace Netfox.FrameworkAPI.Tests
{
        internal class NullLogger : ILogger
        {
            #region Implementation of ILogger
            public ILogger CreateChildLogger(string loggerName) { return null; }
            public void Trace(string message) {  }
            public void Trace(Func<string> messageFactory) {  }
            public void Trace(string message, Exception exception) {  }
            public void TraceFormat(string format, params object[] args) { }
            public void TraceFormat(Exception exception, string format, params object[] args) {  }
            public void TraceFormat(IFormatProvider formatProvider, string format, params object[] args) {  }
            public void TraceFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args) {  }
            public void Debug(string message) { }
            public void Debug(Func<string> messageFactory) { }
            public void Debug(string message, Exception exception) { }
            public void DebugFormat(string format, params object[] args) { }
            public void DebugFormat(Exception exception, string format, params object[] args) { }
            public void DebugFormat(IFormatProvider formatProvider, string format, params object[] args) { }
            public void DebugFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args) { }
            public void Error(string message) { }
            public void Error(Func<string> messageFactory) { }
            public void Error(string message, Exception exception) { }
            public void ErrorFormat(string format, params object[] args) { }
            public void ErrorFormat(Exception exception, string format, params object[] args) { }
            public void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args) { }
            public void ErrorFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args) { }
            public void Fatal(string message) { }
            public void Fatal(Func<string> messageFactory) { }
            public void Fatal(string message, Exception exception) { }
            public void FatalFormat(string format, params object[] args) { }
            public void FatalFormat(Exception exception, string format, params object[] args) { }
            public void FatalFormat(IFormatProvider formatProvider, string format, params object[] args) { }
            public void FatalFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args) { }
            public void Info(string message) { }
            public void Info(Func<string> messageFactory) { }
            public void Info(string message, Exception exception) { }
            public void InfoFormat(string format, params object[] args) { }
            public void InfoFormat(Exception exception, string format, params object[] args) { }
            public void InfoFormat(IFormatProvider formatProvider, string format, params object[] args) { }
            public void InfoFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args) { }
            public void Warn(string message) { }
            public void Warn(Func<string> messageFactory) { }
            public void Warn(string message, Exception exception) { }
            public void WarnFormat(string format, params object[] args) { }
            public void WarnFormat(Exception exception, string format, params object[] args) { }
            public void WarnFormat(IFormatProvider formatProvider, string format, params object[] args) { }
            public void WarnFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args) { }
            public bool IsTraceEnabled { get; }
            public bool IsDebugEnabled { get; }
            public bool IsErrorEnabled { get; }
            public bool IsFatalEnabled { get; }
            public bool IsInfoEnabled { get; }
            public bool IsWarnEnabled { get; }
            #endregion
        }
}