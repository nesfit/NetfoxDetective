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

using System.Collections.ObjectModel;
using System.Windows.Data;
using Castle.Core;
using Castle.Core.Logging;
using log4net.Core;
using Netfox.Core.Collections;

namespace Netfox.Logger
{
    public class NetfoxOutputAppender : NetfoxAppenderBase
    {
        private readonly object _lock = new object();

        public NetfoxOutputAppender() {
            BindingOperations.EnableCollectionSynchronization(this.OutputMessages, this);
        }
        public ConcurrentObservableCollection<LoggingEvent> OutputMessages { get; } = new ConcurrentObservableCollection<LoggingEvent>();

        #region Overrides of NetfoxAppenderBase
        [DoNotWire]
        public override string Name { get; set; } = "Netfox Appender";
        public override void Close() { return; }

        protected override void DoAppendApproved(LoggingEvent loggingEvent)
        {
            lock(this._lock)this.OutputMessages.Add(loggingEvent);
        }
        #endregion
    }
}