using System;

namespace Netfox.Detective.Interfaces
{
    public interface IDetectiveMessenger
    {
        void Register<TMessage>(object recipient, Action<TMessage> action);

        void Send<TMessage>(TMessage message);

        void AsyncSend<TMessage>(TMessage message);
    }
}