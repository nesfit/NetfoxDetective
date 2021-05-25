namespace Netfox.Web.App.ViewModels.Investigation
{
    public sealed class Pair<TKey, TValue>
    {
        public readonly TKey Key;
        public readonly TValue Value;

        public Pair()
        {
            Key = default;
            Value = default;
        }

        public Pair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }
}