using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using AlphaChiTech.VirtualizingCollection;
using AlphaChiTech.VirtualizingCollection.Interfaces;
using Castle.Windsor;

namespace Netfox.Core.Database
{
    public class VirtualizingObservableDBSetPagedCollection<T> : VirtualizingObservableCollection<T>, INotifyImmediately
        where T : class, IEntity
    {
        public VirtualizingObservableDBSetPagedCollection(IWindsorContainer investigationOrAppWindsorContainer,
            IObservableNetfoxDBContext dbContext)
            : base(new PageDbSetSourceProvider<T>(new ObservableDbSetRepository<T>(investigationOrAppWindsorContainer,
                dbContext)))
        {
        }

        public VirtualizingObservableDBSetPagedCollection(IWindsorContainer investigationOrAppWindsorContainer,
            IObservableNetfoxDBContext dbContext, IEnumerable<string> eagerLoadProperties)
            : base(new PageDbSetSourceProvider<T>(new ObservableDbSetRepository<T>(investigationOrAppWindsorContainer,
                dbContext, null, eagerLoadProperties)))
        {
        }

        public VirtualizingObservableDBSetPagedCollection(IWindsorContainer investigationOrAppWindsorContainer,
            IObservableNetfoxDBContext dbContext, Expression<Func<T, Boolean>> filter)
            : base(new PageDbSetSourceProvider<T>(new ObservableDbSetRepository<T>(investigationOrAppWindsorContainer,
                dbContext, filter, null)))
        {
        }

        public VirtualizingObservableDBSetPagedCollection(IWindsorContainer investigationOrAppWindsorContainer,
            IObservableNetfoxDBContext dbContext, Expression<Func<T, Boolean>> filter,
            IEnumerable<string> eagerLoadProperties)
            : base(new PageDbSetSourceProvider<T>(new ObservableDbSetRepository<T>(investigationOrAppWindsorContainer,
                dbContext, filter, eagerLoadProperties)))
        {
        }

        public bool IsNotifyImmidiately
        {
            get => this.Provider is INotifyImmediately iNotifyImmediatelyProvider &&
                   iNotifyImmediatelyProvider.IsNotifyImmidiately;
            set
            {
                if (this.Provider is INotifyImmediately iNotifyImmediatelyProvider)
                {
                    iNotifyImmediatelyProvider.IsNotifyImmidiately = value;
                }
            }
        }
    }
}