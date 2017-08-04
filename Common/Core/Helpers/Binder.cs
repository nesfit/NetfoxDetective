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
using System.ComponentModel;
using System.Reflection;

namespace Netfox.Core.Helpers
{
    public static class Binder
    {

        public static void Bind(
            INotifyPropertyChanged source,
            string sourcePropertyName,
            INotifyPropertyChanged target,
            string targetPropertyName,BindingDirection bindingDirection)
        {
            var sourceProperty
                = source.GetType().GetProperty(sourcePropertyName);
            var targetProperty
                = target.GetType().GetProperty(targetPropertyName);

            switch(bindingDirection)
            {
                case BindingDirection.OneWay:
                    SourceToTarget(source, sourceProperty, target, targetProperty);
                    break;
                case BindingDirection.TwoWay:
                    SourceToTarget(source, sourceProperty, target, targetProperty);
                    SourceToTarget(target, targetProperty, source, sourceProperty);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(bindingDirection), bindingDirection, null);
            }
            //source.PropertyChanged +=
            //    (s, a) =>
            //    {
            //        var sourceValue = sourceProperty.GetValue(source, null);
            //        var targetValue = targetProperty.GetValue(target, null);
            //        if (!Object.Equals(sourceValue, targetValue))
            //        {
            //            targetProperty.SetValue(target, sourceValue, null);
            //        }
            //    };

            //target.PropertyChanged +=
            //    (s, a) =>
            //    {
            //        var sourceValue = sourceProperty.GetValue(source, null);
            //        var targetValue = targetProperty.GetValue(target, null);
            //        if (!Object.Equals(sourceValue, targetValue))
            //        {
            //            sourceProperty.SetValue(source, targetValue, null);
            //        }
            //    };
        }

        private static void SourceToTarget(INotifyPropertyChanged source, PropertyInfo sourceProperty, INotifyPropertyChanged target, PropertyInfo targetProperty)
        {
            source.PropertyChanged +=
                (s, a) =>
                {
                var sourceValue = sourceProperty.GetValue(source, null);
                var targetValue = targetProperty.GetValue(target, null);
                if (!Equals(sourceValue, targetValue))
                {
                    targetProperty.SetValue(target, sourceValue, null);
                }
            };
        }
    }
}
