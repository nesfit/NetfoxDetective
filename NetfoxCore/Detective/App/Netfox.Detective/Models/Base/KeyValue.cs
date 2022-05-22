namespace Netfox.Detective.Models.Base
{
    /// <summary>
    ///     Modifiable key value pair.
    /// </summary>
    /// <typeparam name="TKeyType"></typeparam>
    /// <typeparam name="TValueType"></typeparam>
    public class KeyValue<TKeyType, TValueType>
    {
        #region Constructors

        public KeyValue()
        {
        }

        public KeyValue(TKeyType key, TValueType value)
        {
            this.Key = key;
            this.Value = value;
        }

        public KeyValue(TKeyType key, TValueType value, object oRef)
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