using System;
using System.Collections.Specialized;

namespace AlphaChiTech.Virtualization.Actions
{
    public class ExecuteResetWa<T> : BaseActionVirtualization where T : class
    {
        readonly WeakReference _voc;

        public ExecuteResetWa(VirtualizingObservableCollection<T> voc)
            : base(VirtualActionThreadModelEnum.UseUiThread)
        {
            this._voc = new WeakReference(voc);
        }

        public override void DoAction()
        {
            var voc = (VirtualizingObservableCollection<T>)this._voc.Target;

            if (voc != null && this._voc.IsAlive)
            {
                voc.RaiseCollectionChangedEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

    }
}
