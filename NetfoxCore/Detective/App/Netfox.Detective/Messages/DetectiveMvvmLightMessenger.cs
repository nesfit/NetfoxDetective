using System;
using System.Threading;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using Netfox.Detective.Interfaces;

namespace Netfox.Detective.Messages
{
    public class DetectiveMvvmLightMessenger : IDetectiveMessenger
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
            DispatcherHelper.UIDispatcher?.BeginInvoke(new ThreadStart(() => this._messenger.Send(message)),
                DispatcherPriority.Send);
        }
    }
}