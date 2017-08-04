#region

#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq.Dynamic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DataGridAsyncDemo.filtersort;

namespace DataGridAsyncDemo
{
    /// <summary>
    ///     Emulate a remote data repository (list of item + sort & filter values)
    /// </summary>
    public class RemoteOrDbDataSourceEmulation : IFilteredSortedSourceProviderAsync, INotifyCollectionChanged
    {
        #region statics

        //private static RemoteOrDbDataSourceEmulation _instance;
        private static readonly object _syncRoot = new object();

        #endregion

        public RemoteOrDbDataSourceEmulation()
        {
            for (; items < 100000; items++)
            {
                _items.Add(new RemoteOrDbDataItem("Name_" + items, "Str1_" + items, "Str1_" + items, items, items));
                Interlocked.Increment(ref items);
            }

            SortDescriptionList.CollectionChanged += SortDescriptionListOnCollectionChanged;
            FilterDescriptionList.CollectionChanged += FilterDescriptionListOnCollectionChanged;
            _items.CollectionChanged += (sender, args) =>
            {
                CollectionChanged?.Invoke(this, args);
                
            };

            Task.Run(() =>
            {
                while (true)
                {
                    _items.Add(new RemoteOrDbDataItem("Name_" + items, "Str1_" + items, "Str1_" + items, items, items));
                    Task.Delay(100).Wait();
                    Interlocked.Increment(ref items);
                }
                
            });
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #region fields

        private readonly ObservableCollection<RemoteOrDbDataItem> _items =
            new ObservableCollection<RemoteOrDbDataItem>();

        private readonly List<RemoteOrDbDataItem> _orderedItems = new List<RemoteOrDbDataItem>();
        private bool _isFilteredItemsValid;
        private string _orderByLinqExpression = "";
        private string _whereLinqExpression = "";

        private int items;

        #endregion

        #region properties

        public IList<RemoteOrDbDataItem> FilteredOrderedItems
        {
            get
            {
                if (!_isFilteredItemsValid)
                {
                    lock (this)
                    {
                        _orderedItems.Clear();

                        try
                        {
                            if (string.IsNullOrWhiteSpace(WhereLinqExpression) &&
                                string.IsNullOrWhiteSpace(OrderByLinqExpression))
                                _orderedItems.AddRange(_items);
                            else if (!string.IsNullOrWhiteSpace(WhereLinqExpression) &&
                                     string.IsNullOrWhiteSpace(OrderByLinqExpression))
                                _orderedItems.AddRange(_items.Where(WhereLinqExpression));
                            else if (string.IsNullOrWhiteSpace(WhereLinqExpression) &&
                                     !string.IsNullOrWhiteSpace(OrderByLinqExpression))
                                _orderedItems.AddRange(_items.OrderBy(OrderByLinqExpression));
                            else if (!string.IsNullOrWhiteSpace(WhereLinqExpression) &&
                                     !string.IsNullOrWhiteSpace(OrderByLinqExpression))
                                _orderedItems.AddRange(
                                    _items.Where(WhereLinqExpression).OrderBy(OrderByLinqExpression));
                        }
                        catch (Exception ex)
                        {
                            var aa = 0;
                        }
                        _isFilteredItemsValid = true;
                    }
                }
                return _orderedItems;
            }
        }

        public string OrderByLinqExpression
        {
            get { return _orderByLinqExpression; }
            set
            {
                if (!string.Equals(_orderByLinqExpression, value))
                {
                    _orderByLinqExpression = value;
                    _isFilteredItemsValid = false;
                }
            }
        }

        public string WhereLinqExpression
        {
            get { return _whereLinqExpression; }
            set
            {
                if (!string.Equals(_whereLinqExpression, value))
                {
                    _whereLinqExpression = value;
                    _isFilteredItemsValid = false;
                }
            }
        }

        #endregion

        #region public members

        public void OrderBy(string orderByExpression)
        {
            if (!string.Equals(orderByExpression, OrderByLinqExpression))
                OrderByLinqExpression = orderByExpression;
        }

        public void Where(string whereExpression)
        {
            if (!string.Equals(whereExpression, WhereLinqExpression))
                WhereLinqExpression = whereExpression;
        }

        #endregion

        #region filter & sort Descrioption list

        public SortDescriptionList SortDescriptionList { get; } = new SortDescriptionList();

        public FilterDescriptionList FilterDescriptionList { get; } = new FilterDescriptionList();

        private void SortDescriptionListOnCollectionChanged(object sender,
            NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            var sort = "";

            var sortFound = false;
            foreach (var sortDescription in SortDescriptionList)
            {
                if (sortFound)
                    sort += ", ";

                sortFound = true;

                sort += sortDescription.PropertyName;
                sort += (sortDescription.Direction == ListSortDirection.Ascending) ? " ASC" : " DESC";
            }

            //if ((!sortFound) && (!string.IsNullOrWhiteSpace( primaryKey )))
            //  sort += primaryKey + " ASC";

            OrderByLinqExpression = sort;
        }

        private void FilterDescriptionListOnCollectionChanged(object sender,
            NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Reset)
            {
                var filter = "";

                var filterFound = false;
                foreach (var filterDescription in FilterDescriptionList)
                {
                    var subFilter = GetLinqQueryString(filterDescription);
                    if (!string.IsNullOrWhiteSpace(subFilter))
                    {
                        if (filterFound)
                            filter += " and ";
                        filterFound = true;
                        filter += " " + subFilter + " ";
                    }
                }

                WhereLinqExpression = filter;
            }
        }

        #region query builder

        private static readonly Regex _regexSplit =
            new Regex(
                @"(and)|(or)|(==)|(<>)|(!=)|(<=)|(>=)|(&&)|(\|\|)|(=)|(>)|(<)|(\*[\-_a-zA-Z0-9]+)|([\-_a-zA-Z0-9]+\*)|([\-_a-zA-Z0-9]+)",
                RegexOptions.IgnoreCase);

        private static readonly Regex _regexOp =
            new Regex(@"(and)|(or)|(==)|(<>)|(!=)|(<=)|(>=)|(&&)|(\|\|)|(=)|(>)|(<)", RegexOptions.IgnoreCase);

        private static readonly Regex _regexComparOp = new Regex(@"(==)|(<>)|(!=)|(<=)|(>=)|(=)|(>)|(<)",
            RegexOptions.None);

        private static string GetLinqQueryString(FilterDescription filterDescription)
        {
            var ret = "";

            if (!string.IsNullOrWhiteSpace(filterDescription.Filter))
            {
                // using user str + linq.dynamic
                try
                {
                    // xceed syntax : empty (contains), AND (uppercase), OR (uppercase), <>, * (end with), =, >, >=, <, <=, * (start with)
                    //    see http://doc.xceedsoft.com/products/XceedWpfDataGrid/Filter_Row.html 
                    // linq.dynamic syntax : =, ==, <>, !=, <, >, <=, >=, &&, and, ||, or, x.m(…) (where x is the attrib and m the function (ex: Contains, StartsWith, EndsWith ...)
                    //    see D:\DevC#\VirtualisingCollectionTest1\DynamicQuery\Dynamic Expressions.html 
                    // ex : RemoteOrDbDataSourceEmulation.Instance.Items.Where( "Name.Contains(\"e_1\") or Name.Contains(\"e_2\")" );

                    var exp = filterDescription.Filter;

                    // arrange expression

                    var previousTermIsOperator = false;
                    foreach (Match match in _regexSplit.Matches(exp))
                    {
                        if (match.Success)
                        {
                            //TODO processing results
                            if (_regexOp.IsMatch(match.Value))
                            {
                                if (_regexComparOp.IsMatch(match.Value))
                                {
                                    // simple operator >, <, ==, != ...
                                    ret += " " + filterDescription.PropertyName + " " + match.Value;
                                    previousTermIsOperator = true;
                                }
                                else
                                {
                                    // and, or ...
                                    ret += " " + match.Value;
                                    previousTermIsOperator = false;
                                }
                            }
                            else
                            {
                                // Value
                                if (previousTermIsOperator)
                                {
                                    ret += " " + match.Value;
                                    previousTermIsOperator = false;
                                }
                                else
                                {
                                    if (match.Value.StartsWith("*"))
                                        ret += " " + filterDescription.PropertyName + ".EndsWith( \"" +
                                               match.Value.Substring(1) + "\" )";
                                    else if (match.Value.EndsWith("*"))
                                        ret += " " + filterDescription.PropertyName + ".StartsWith( \"" +
                                               match.Value.Substring(0, match.Value.Length - 1) + "\" )";
                                    else
                                        ret += " " + filterDescription.PropertyName + ".Contains( \"" + match.Value +
                                               "\" )";
                                    previousTermIsOperator = false;
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }
            }

            return ret;
        }

        #endregion query builder

        #endregion filter & sort Descrioption list
    }
}