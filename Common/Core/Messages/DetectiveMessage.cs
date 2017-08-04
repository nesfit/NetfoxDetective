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
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;

namespace Netfox.Core.Messages
{
    /// <summary>
    ///     Base class from communication using messages in application
    /// </summary>
    public abstract class DetectiveMessage : MessageBase
    {
        public enum SendMethod
        {
            /// Using dispatcher, safe for non GUI threads
            Async,

            /// Instantious evaluation, blocks GUI, only from GUI thread
            Blocking
        }

        /// <summary>
        ///     Send message of specified type using UI Dispatcher with highest possible priority
        /// </summary>
        /// <typeparam name="TMessageType"></typeparam>
        /// <param name="message"></param>
        protected static void AsyncSendMessage<TMessageType>(TMessageType message)
        {
            
            try { DispatcherHelper.UIDispatcher?.BeginInvoke(new ThreadStart(() => Messenger.Default.Send(message)), DispatcherPriority.Send); }
            catch (Exception)
            {
                Debugger.Break();
            }
        }

        protected static void BlockingSendMessage<TMessageType>(TMessageType message)
        {
            try { Messenger.Default.Send(message); }
            catch(Exception)
            {
                Debugger.Break();
            }
        }
    }
}