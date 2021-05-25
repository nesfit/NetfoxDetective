﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using AlphaChiTech.VirtualizingCollection.Pageing;
using AlphaChiTech.VirtualizingCollection;
using AlphaChiTech.VirtualizingCollection.Interfaces;
using Castle.Core.Logging;
using Castle.Windsor;
using Netfox.Core.Interfaces;
using Netfox.Core.Interfaces.ViewModels;

namespace Netfox.Core.Collections
{
    public class ViewModelVirtualizingIoCObservableCollection<TVmType, TModelType>: VirtualizingObservableCollection<TVmType>, INotifyCollectionChanged, INotifyPropertyChanged,
        IReadOnlyCollection<TVmType>, ILoggable where TModelType : class where TVmType : class, IDataEntityVm
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="VirtualizingObservableCollection{T}" /> class.
        /// </summary>
        /// <param name="provider">The provider.</param>
        public ViewModelVirtualizingIoCObservableCollection(IItemSourceProvider<TVmType> provider) : base(provider)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="VirtualizingObservableCollection{T}" /> class.
        /// </summary>
        /// <param name="asyncProvider">The asynchronous provider.</param>
        public ViewModelVirtualizingIoCObservableCollection(IItemSourceProviderAsync<TVmType> asyncProvider) : base(asyncProvider)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="VirtualizingObservableCollection{T}" /> class.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="reclaimer">The optional reclaimer.</param>
        /// <param name="expiryComparer">The optional expiry comparer.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="maxPages">The maximum pages.</param>
        /// <param name="maxDeltas">The maximum deltas.</param>
        /// <param name="maxDistance">The maximum distance.</param>
        public ViewModelVirtualizingIoCObservableCollection(
            IPagedSourceProvider<TVmType> provider,
            IPageReclaimer<TVmType> reclaimer = null,
            IPageExpiryComparer expiryComparer = null,
            int pageSize = 100,
            int maxPages = 100,
            int maxDeltas = -1,
            int maxDistance = -1) : base(provider, reclaimer, expiryComparer, pageSize, maxPages, maxDeltas, maxDistance)
        {
            throw new NotImplementedException();
        }

        public ViewModelVirtualizingIoCObservableCollection(IObservableCollection<TModelType> sourceCollection, IWindsorContainer globalOrInvestigationWindsorContainer)
            : base(
                new VirtualizingObservableCollectionTransformingPagedSource<TVmType, TModelType>(sourceCollection, model => CreateVm(model, globalOrInvestigationWindsorContainer),
                    (TVmType vm) => vm.EncapsulatedModel as TModelType))
        {
            this.VirtualizingObservableCollectionTransformingPagedSource =
                (this.Provider as PaginationManager<TVmType>)?.Provider as VirtualizingObservableCollectionTransformingPagedSource<TVmType, TModelType>;
        }

        public ViewModelVirtualizingIoCObservableCollection(IObservableCollection sourceCollection, IWindsorContainer globalOrInvestigationWindsorContainer)
            : base(
                new VirtualizingObservableCollectionTransformingPagedSource<TVmType, TModelType>(sourceCollection, model => CreateVm(model, globalOrInvestigationWindsorContainer),
                    (TVmType vm) => vm.EncapsulatedModel as TModelType))
        {
            this.VirtualizingObservableCollectionTransformingPagedSource =
                (this.Provider as PaginationManager<TVmType>)?.Provider as VirtualizingObservableCollectionTransformingPagedSource<TVmType, TModelType>;
        }

        public ViewModelVirtualizingIoCObservableCollection(ICollection<TModelType> sourceCollection, IWindsorContainer globalOrInvestigationWindsorContainer)
            : base(
                new VirtualizingObservableCollectionTransformingPagedSource<TVmType, TModelType>(
                    new ConcurrentIObservableVirtualizingObservableCollection<TModelType>(sourceCollection), model => CreateVm(model, globalOrInvestigationWindsorContainer),
                    (TVmType vm) => vm.EncapsulatedModel as TModelType))
        {
            this.VirtualizingObservableCollectionTransformingPagedSource =
                (this.Provider as PaginationManager<TVmType>)?.Provider as VirtualizingObservableCollectionTransformingPagedSource<TVmType, TModelType>;
        }

        public WindsorContainer GlobalOrInvestigationWindsorContainer { get; set; }
        public VirtualizingObservableCollectionTransformingPagedSource<TVmType, TModelType> VirtualizingObservableCollectionTransformingPagedSource { get; }

        public void Add(TModelType item) { this.VirtualizingObservableCollectionTransformingPagedSource?.OnAppend(item, DateTime.Now); }

        public void Remove(TModelType item) { this.VirtualizingObservableCollectionTransformingPagedSource?.OnRemove(item, DateTime.Now); }

        private  static TVmType CreateVm(TModelType model, IWindsorContainer globalOrInvestigationWindsorContainer)
        {
            var logger = globalOrInvestigationWindsorContainer.Resolve<ILogger>();
            if(model == null)
            {
                logger?.Error("Vm instantiation have failed - Model cannot be NULL");
                throw new ArgumentNullException(nameof(model), "Model cannot be NULL");
            }
            try
            {
                var wc = globalOrInvestigationWindsorContainer;
                if(model is IWindsorContainerChanger) { wc = (model as IWindsorContainerChanger)?.InvestigationWindsorContainer; }
               
                if(wc==null) Debugger.Break();
                var newVm = wc.Resolve<TVmType>(new Dictionary<string, object>
                {
                  { "investigationOrAppWindsorContainer", wc },
                  { "model", model }
                });
                if(newVm == null) Debugger.Break();

                (newVm as Interfaces.IInitializable)?.Initialize();
                return newVm;
            }
            catch(Exception ex)
            {
                logger?.Error("Vm instantiation have failed - Model cannot be NULL",ex);
                Debugger.Break();
                throw;
            }
        }

        #region Implementation of ILoggable
        public ILogger Logger { get; set; }
        #endregion
    }
}