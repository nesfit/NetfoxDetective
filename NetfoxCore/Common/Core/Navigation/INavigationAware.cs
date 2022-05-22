namespace Netfox.Core.Navigation
{
    public interface INavigationAware
    {
        void OnNavigatedFrom();
        void OnNavigatedTo();
    }
}