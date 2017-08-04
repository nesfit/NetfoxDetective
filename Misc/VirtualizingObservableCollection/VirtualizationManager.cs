using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlphaChiTech.Virtualization.Actions;
using AlphaChiTech.Virtualization.Interfaces;

namespace AlphaChiTech.Virtualization
{
    public class VirtualizationManager
    {
        private readonly List<IVirtualizationAction> _actions = new List<IVirtualizationAction>();
        private readonly object _actionLock = new object();

        public static bool IsInitialized { get; private set; } = false;

        public static VirtualizationManager Instance { get; } = new VirtualizationManager();

        bool _processing = false;

        private Action<Action> _uiThreadExcecuteAction = null;

        public Action<Action> UiThreadExcecuteAction
        {
            get { return this._uiThreadExcecuteAction; }
            set
            {
                this._uiThreadExcecuteAction = value;
                IsInitialized = true;
            }
        }

        public void ProcessActions()
        {
            if (this._processing) return;

            this._processing = true;

            List<IVirtualizationAction> lst;
            lock (this._actionLock)
            {
                lst = this._actions.ToList();
            }

            foreach (var action in lst)
            {
                var bdo = true;

                if (action is IRepeatingVirtualizationAction)
                {
                    bdo = (action as IRepeatingVirtualizationAction).IsDueToRun();
                }

                if (bdo)
                {
                    switch (action.ThreadModel)
                    {
                        case VirtualActionThreadModelEnum.UseUiThread:
                            if (this.UiThreadExcecuteAction == null) // PLV
                              throw new Exception( "VirtualizationManager isn’t already initialized !  set the VirtualizationManager’s UIThreadExcecuteAction (VirtualizationManager.Instance.UIThreadExcecuteAction = a => Dispatcher.Invoke( a );)" );
                            this.UiThreadExcecuteAction.Invoke(() => action.DoAction());
                            break;
                        case VirtualActionThreadModelEnum.Background:
                            Task.Run(() => action.DoAction());
                            break;
                    }

                    if (action is IRepeatingVirtualizationAction)
                    {
                        if (!(action as IRepeatingVirtualizationAction).KeepInActionsList())
                        {
                            lock (this._actionLock)
                            {
                                this._actions.Remove(action);
                            }
                        }
                    }
                    else
                    {
                        lock (this._actionLock)
                        {
                            this._actions.Remove(action);
                        }
                    }
                }
            }

            this._processing = false;
        }
        public void AddAction(IVirtualizationAction action)
        {
            lock (this._actionLock)
            {
                this._actions.Add(action);
            }
        }

        public void AddAction(Action action)
        {
            this.AddAction(new ActionVirtualizationWrapper(action));
        }

        public void RunOnUI(IVirtualizationAction action)
        {
            if (this.UiThreadExcecuteAction == null) // PLV
               throw new Exception( "VirtualizationManager isn’t already initialized !  set the VirtualizationManager’s UIThreadExcecuteAction (VirtualizationManager.Instance.UIThreadExcecuteAction = a => Dispatcher.Invoke( a );)" );
            this.UiThreadExcecuteAction.Invoke(() => action.DoAction());
        }

        public void RunOnUI(Action action)
        {
            this.RunOnUI(new ActionVirtualizationWrapper(action));
        }
    }
}
