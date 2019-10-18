using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Netfox.SnooperDNS.Models.Message
{
    [ComplexType]
    public class DNSQuery : DNSBase
    {
        public override String ToString() { return $"{base.ToString()}"; }
    }
}