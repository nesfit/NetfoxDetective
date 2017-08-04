// Copyright (c) 2017 Jan Pluskal, Martin Mares, Martin Kmet
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;

namespace Netfox.Detective.Core
{
    /// <summary>
    ///     Monitors the PropertyChanged event of an object that implements INotifyPropertyChanged,
    ///     and executes callback methods (i.e. handlers) registered for properties of that object.
    ///     http://joshsmithonwpf.wordpress.com/2009/07/11/one-way-to-avoid-messy-propertychanged-event-handling/
    /// </summary>
    /// <typeparam name="TPropertySource">The type of object to monitor for property changes.</typeparam>
    public class PropertyObserver<TPropertySource> : IWeakEventListener where TPropertySource : INotifyPropertyChanged
    {
        #region Constructor
        /// <summary>
        ///     Initializes a new instance of PropertyObserver, which
        ///     observes the 'propertySource' object for property changes.
        /// </summary>
        /// <param name="propertySource">The object to monitor for property changes.</param>
        public PropertyObserver(TPropertySource propertySource)
        {
            if(propertySource == null) { throw new ArgumentNullException("propertySource"); }

            this._propertySourceRef = new WeakReference(propertySource);
            this._propertyNameToHandlerMap = new Dictionary<string, Action<TPropertySource>>();
        }
        #endregion // Constructor

        #region IWeakEventListener Members
        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if(managerType == typeof(PropertyChangedEventManager))
            {
                var propertyName = ((PropertyChangedEventArgs) e).PropertyName;
                var propertySource = (TPropertySource) sender;

                if(String.IsNullOrEmpty(propertyName))
                {
                    // When the property name is empty, all properties are considered to be invalidated.
                    // Iterate over a copy of the list of handlers, in case a handler is registered by a callback.
                    foreach(var handler in this._propertyNameToHandlerMap.Values.ToArray()) { handler(propertySource); }

                    return true;
                }
                Action<TPropertySource> actionhandler;
                if(this._propertyNameToHandlerMap.TryGetValue(propertyName, out actionhandler))
                {
                    actionhandler(propertySource);
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region Public Methods

        #region RegisterHandler
        /// <summary>
        ///     Registers a callback to be invoked when the PropertyChanged event has been raised for the specified property.
        /// </summary>
        /// <param name="expression">A lambda expression like 'n => n.PropertyName'.</param>
        /// <param name="handler">The callback to invoke when the property has changed.</param>
        /// <returns>The object on which this method was invoked, to allow for multiple invocations chained together.</returns>
        public PropertyObserver<TPropertySource> RegisterHandler(Expression<Func<TPropertySource, object>> expression, Action<TPropertySource> handler)
        {
            if(expression == null) { throw new ArgumentNullException("expression"); }

            var propertyName = this.GetPropertyName(expression);
            if(String.IsNullOrEmpty(propertyName)) { throw new ArgumentException("'expression' did not provide a property name."); }

            if(handler == null) { throw new ArgumentNullException("handler"); }

            var propertySource = this.GetPropertySource();
            if(propertySource != null)
            {
                this._propertyNameToHandlerMap[propertyName] = handler;
                PropertyChangedEventManager.AddListener(propertySource, this, propertyName);
            }

            return this;
        }
        #endregion // RegisterHandler

        #region UnregisterHandler
        /// <summary>
        ///     Removes the callback associated with the specified property.
        /// </summary>
        /// <param name="propertyName">A lambda expression like 'n => n.PropertyName'.</param>
        /// <returns>The object on which this method was invoked, to allow for multiple invocations chained together.</returns>
        public PropertyObserver<TPropertySource> UnregisterHandler(Expression<Func<TPropertySource, object>> expression)
        {
            if(expression == null) { throw new ArgumentNullException("expression"); }

            var propertyName = this.GetPropertyName(expression);
            if(String.IsNullOrEmpty(propertyName)) { throw new ArgumentException("'expression' did not provide a property name."); }

            var propertySource = this.GetPropertySource();
            if(propertySource != null)
            {
                if(this._propertyNameToHandlerMap.ContainsKey(propertyName))
                {
                    this._propertyNameToHandlerMap.Remove(propertyName);
                    PropertyChangedEventManager.RemoveListener(propertySource, this, propertyName);
                }
            }

            return this;
        }
        #endregion // UnregisterHandler

        #endregion // Public Methods

        #region Private Helpers

        #region GetPropertyName
        public string GetPropertyName(Expression<Func<TPropertySource, object>> expression)
        {
            var lambda = expression as LambdaExpression;
            MemberExpression memberExpression;
            if(lambda.Body is UnaryExpression)
            {
                var unaryExpression = lambda.Body as UnaryExpression;
                memberExpression = unaryExpression.Operand as MemberExpression;
            }
            else
            { memberExpression = lambda.Body as MemberExpression; }

            Debug.Assert(memberExpression != null, "Please provide a lambda expression like 'n => n.PropertyName'");

            if(memberExpression != null)
            {
                var propertyInfo = memberExpression.Member as PropertyInfo;

                return propertyInfo.Name;
            }

            return null;
        }
        #endregion // GetPropertyName

        #region GetPropertySource
        private TPropertySource GetPropertySource()
        {
            try { return (TPropertySource) this._propertySourceRef.Target; }
            catch {
                return default(TPropertySource);
            }
        }
        #endregion // GetPropertySource

        #endregion // Private Helpers

        #region Fields
        private readonly Dictionary<string, Action<TPropertySource>> _propertyNameToHandlerMap;
        private readonly WeakReference _propertySourceRef;
        #endregion // Fields
    }
}