using System.Collections.Specialized;

namespace VirtualizingObservableCollectionDemo.filtersort
{
    public class FilterDescriptionList : DescriptionList<FilterDescription>
  {
    public void OnCollectionReset()
    {
        this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
    }
  }
}