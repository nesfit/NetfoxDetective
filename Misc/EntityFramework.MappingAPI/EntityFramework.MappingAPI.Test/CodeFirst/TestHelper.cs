using System.Linq.Expressions;
using NUnit.Framework;

namespace EntityFramework.MappingAPI.Test.CodeFirst
{
    public static class TestHelper
    {
        public static IPropertyMap HasColumnName(this IPropertyMap c, string columnName)
        {
            string message = string.Format("Property {0} should be mapped to col {1}, but was mapped to {2}", c.PropertyName, columnName, c.ColumnName);
            Assert.AreEqual(columnName, c.ColumnName, message);
            return c;
        }

        public static IPropertyMap IsPk(this IPropertyMap c, bool isPk = true)
        {
            string message = string.Format("Property {0} pk flag should be {1}, but was {2}", c.PropertyName, isPk, c.IsPk);
            Assert.AreEqual(isPk, c.IsPk, message);
            return c;
        }

        public static IPropertyMap IsFk(this IPropertyMap c, bool isFk = true)
        {
            string message = string.Format("Property {0} fk flag should be {1}, but was {2}", c.PropertyName, isFk, c.IsFk);
            Assert.AreEqual(isFk, c.IsFk, message);
            return c;
        }

        public static IPropertyMap FixedLength(this IPropertyMap c, bool fixedLength = true)
        {
            string message = string.Format("Property {0} fixedLength flag should be {1}, but was {2}", c.PropertyName, fixedLength, c.FixedLength);
            Assert.AreEqual(fixedLength, c.FixedLength, message);
            return c;
        }

        public static IPropertyMap Unicode(this IPropertyMap c, bool unicode = true)
        {
            string message = string.Format("Property {0} unicode flag should be {1}, but was {2}", c.PropertyName, unicode, c.Unicode);
            Assert.AreEqual(unicode, c.Unicode, message);
            return c;
        }

        public static IPropertyMap IsNavigationProperty(this IPropertyMap c, bool isNavProp = true)
        {
            string message = string.Format("Property {0} navigationProperty flag should be {1}, but was {2}", c.PropertyName, isNavProp, c.IsNavigationProperty);
            Assert.AreEqual(isNavProp, c.IsNavigationProperty, message);
            return c;
        }

        public static IPropertyMap MaxLength(this IPropertyMap c, int maxLength)
        {
            string message = string.Format("Property {0} max length should be {1}, but was {2}", c.PropertyName, maxLength, c.MaxLength);
            Assert.AreEqual(maxLength, c.MaxLength, message);
            return c;
        }

        public static IPropertyMap NavigationPropertyName(this IPropertyMap c, string navigationProperty)
        {
            string message = string.Format("Property {0} navigation property should be '{1}', but was '{2}'", c.PropertyName, navigationProperty, c.NavigationPropertyName);
            Assert.AreEqual(navigationProperty, c.NavigationPropertyName, message);
            return c;
        }

        public static IPropertyMap ForeignKeyPropertyName(this IPropertyMap c, string fkProperty)
        {
            string message = string.Format("Property {0} fk property should be '{1}', but was '{2}'", c.PropertyName, fkProperty, c.ForeignKeyPropertyName);
            Assert.AreEqual(fkProperty, c.ForeignKeyPropertyName, message);
            return c;
        }

        public static IPropertyMap ForeignKey(this IPropertyMap c, IPropertyMap fk)
        {
            string message = string.Format("Property {0} fk does not match", c.PropertyName);
            Assert.AreEqual(fk, c.ForeignKey, message);
            return c;
        }

        public static IPropertyMap NavigationProperty(this IPropertyMap c, IPropertyMap navigationProperty)
        {
            string message = string.Format("Property {0} navigation property does not match", c.PropertyName);
            Assert.AreEqual(navigationProperty, c.NavigationProperty, message);
            return c;
        }

        public static IPropertyMap IsDiscriminator(this IPropertyMap c, bool isDiscriminator = true)
        {
            string message = string.Format("Property {0} discriminator flag should be '{1}', but was '{2}'", c.PropertyName, isDiscriminator, c.IsDiscriminator);
            Assert.AreEqual(isDiscriminator, c.IsDiscriminator, message);
            return c;
        }

        public static IPropertyMap IsIdentity(this IPropertyMap c, bool isIdentity = true)
        {
            string message = string.Format("Property {0} identity flag should be '{1}', but was '{2}'", c.PropertyName, isIdentity, c.IsIdentity);
            Assert.AreEqual(isIdentity, c.IsIdentity, message);
            return c;
        }

        public static IPropertyMap IsRequired(this IPropertyMap c, bool isRequired = true)
        {
            string message = string.Format("Property {0} required flag should be '{1}', but was '{2}'", c.PropertyName, isRequired, c.IsRequired);
            Assert.AreEqual(isRequired, c.IsRequired, message);
            return c;
        }

        public static IPropertyMap HasPrecision(this IPropertyMap c, byte precision)
        {
            string message = string.Format("Property {0} precision should be '{1}', but was '{2}'", c.PropertyName, precision, c.Precision);
            Assert.AreEqual(precision, c.Precision, message);
            return c;
        }

        public static IPropertyMap HasScale(this IPropertyMap c, byte scale)
        {
            string message = string.Format("Property {0} scale should be '{1}', but was '{2}'", c.PropertyName, scale, c.Scale);
            Assert.AreEqual(scale, c.Scale, message);
            return c;
        }
    }
}