using System;
using System.ComponentModel.DataAnnotations.Schema;
using Castle.Core.Internal;

namespace Netfox.Framework.Models
{
    [ComplexType]
    public class CypherKey
    {
        public bool IsSet => !this.ServerPrivateKey.IsNullOrEmpty();
        public String ServerPrivateKey { get; set; } = string.Empty;
    }
}