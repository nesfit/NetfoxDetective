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
using System.Threading;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;

namespace Netfox.Detective.Messages
{
    public class DetectiveMvvmLightMessenger: IDetectiveMessenger
    {
        private readonly IMessenger _messenger = Messenger.Default;
        public void Register<TMessage>(object recipient, Action<TMessage> action)
        {
            this._messenger.Register<TMessage>(recipient, action);
        }

        public void Send<TMessage>(TMessage message)
        {
            _messenger.Send(message); 
        }

        public void AsyncSend<TMessage>(TMessage message)
        {
            DispatcherHelper.UIDispatcher?.BeginInvoke(new ThreadStart(() => this._messenger.Send(message)), DispatcherPriority.Send);
        }
    }
}
