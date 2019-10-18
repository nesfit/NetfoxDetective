using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netfox.Web.BL.DTO
{
    public class KeyValueDTO<TKeyType, TValueType>
    {
        #region Constructors
        public KeyValueDTO() { }

        public KeyValueDTO(TKeyType key, TValueType value)
        {
            this.Key = key;
            this.Value = value;
        }

        public KeyValueDTO(TKeyType key, TValueType value, object oRef)
        {
            this.Key = key;
            this.Value = value;
            this.Ref = oRef;
        }
        #endregion

        #region Properties
        public object Ref { get; private set; }
        public TKeyType Key { get; private set; }
        public TValueType Value { get; set; }
        #endregion
    }
}
