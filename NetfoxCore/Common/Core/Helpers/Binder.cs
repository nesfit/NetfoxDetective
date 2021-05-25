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
            string targetPropertyName, BindingDirection bindingDirection)
        {
            var sourceProperty
                = source.GetType().GetProperty(sourcePropertyName);
            var targetProperty
                = target.GetType().GetProperty(targetPropertyName);

            switch (bindingDirection)
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
        }

        private static void SourceToTarget(INotifyPropertyChanged source, PropertyInfo sourceProperty,
            INotifyPropertyChanged target, PropertyInfo targetProperty)
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