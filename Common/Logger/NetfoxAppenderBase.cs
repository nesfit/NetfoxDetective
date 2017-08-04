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
using Castle.Core;
using Castle.Core.Logging;
using log4net.Appender;
using log4net.Core;

namespace Netfox.Logger
{
    public abstract class NetfoxAppenderBase : IAppender
    {
        [DoNotWire]
        public LoggerLevel LoggerLevel { get; set; } = LoggerLevel.Info;
        [DoNotWire]
        public abstract string Name { get; set; }
        public abstract void Close();
        protected abstract void DoAppendApproved(LoggingEvent loggingEvent);

        public void DoAppend(LoggingEvent loggingEvent)
        {
            if(this.LoggerLevel2Level(this.LoggerLevel) <= loggingEvent.Level)
                if(loggingEvent.RenderedMessage!=null)
                this.DoAppendApproved(loggingEvent);
        }

        public Level LoggerLevel2Level(LoggerLevel loggerLevel)
        {
            switch(loggerLevel)
            {
                case LoggerLevel.Off:
                    return Level.Off;
                case LoggerLevel.Fatal:
                    return Level.Fatal;
                case LoggerLevel.Error:
                    return Level.Error;
                case LoggerLevel.Warn:
                    return Level.Warn;
                case LoggerLevel.Info:
                    return Level.Info;
                case LoggerLevel.Debug:
                    return Level.Debug;
                default:
                    return Level.All;
            }
        }
    }
}