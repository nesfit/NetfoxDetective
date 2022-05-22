namespace Netfox.Core.Interfaces
{
    /// <summary>
    ///     This interface should be implemented by all important application classes.
    ///     Interface provides component name.
    /// </summary>
    public interface ISystemComponent
    {
        string ComponentName { get; }
    }
}