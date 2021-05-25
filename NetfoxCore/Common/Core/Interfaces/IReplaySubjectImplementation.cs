using System;
using System.Reactive.Subjects;

namespace Netfox.Core.Interfaces
{
    public interface IReplaySubjectImplementation<T> : ISubject<T>, ISubject<T, T>, IObserver<T>, IObservable<T>, IDisposable
    {
        bool HasObservers { get; }

        void Unsubscribe(IObserver<T> observer);
    
        
    }
}