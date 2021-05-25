using System;
using System.Threading;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using Netfox.Core.Enums;
using Netfox.Core.Interfaces;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModelsDataEntity.BkTasks
{
    [NotifyPropertyChanged]
    public class BgTaskVm : DetectiveDataEntityViewModelBase, IBgTask
    {
        public BgTaskVm(string title, string description, Action<BgTaskVm> taskFinishedCallBack) : base(null)
        {
            this.IsIndeterminate = true;
            this.Title = title;
            this.Description = description;
            this.State = TaskState.Running;
            this.StartTimeStamp = DateTime.Now;
            this._taskFinishedCallBack = taskFinishedCallBack;
        }

        [IgnoreAutoChangeNotification]
        public RelayCommand CAbortTask => this._cAbortTask ??
                                          (this._cAbortTask = new RelayCommand(this.Abort,
                                              () => this.State == TaskState.Running));

        public virtual void Abort()
        {
            throw new NotImplementedException();
        }

        public virtual void Finish(TaskResultState resultState)
        {
            if (this.State != TaskState.Running)
            {
                return;
            }

            this.ResultState = resultState;

            this.State = resultState == TaskResultState.Ok ? TaskState.DoneOk : TaskState.DoneError;

            this.CompleteProgressValue = 100;
            this.Progress = 100;
            this.IsIndeterminate = false;
            this.UpdateDuration();
            this.OnPropertyChanged(nameof(this.State));
            this.OnPropertyChanged(nameof(this.IsActive));
            this._taskFinishedCallBack?.Invoke(this);
        }

        #region Progress reporting

        public void UpdateDuration()
        {
            this.Duration = DateTime.Now - this.StartTimeStamp;
        }

        #endregion

        #region Properties

        private int _completeProgressValue;

        public int CompleteProgressValue
        {
            get => this._completeProgressValue;
            set
            {
                this._completeProgressValue = value;
                this.OnPropertyChanged();
            }
        }

        private int _progress;

        public int Progress
        {
            get => this._progress;
            set
            {
                this._progress = value;
                this.OnPropertyChanged();
            }
        }

        private string _title;

        public string Title
        {
            get => this._title;
            set
            {
                this._title = value;
                this.OnPropertyChanged();
            }
        }

        private string _description;

        public string Description
        {
            get => this._description;
            set
            {
                this._description = value;
                this.OnPropertyChanged();
            }
        }

        private bool _isIndeterminate;
        private RelayCommand _cAbortTask;
        private TaskState _state = TaskState.Ready;
        private TimeSpan _duration = new TimeSpan(0);
        private readonly Action<BgTaskVm> _taskFinishedCallBack;

        public bool IsIndeterminate
        {
            get => this._isIndeterminate;
            set
            {
                this._isIndeterminate = value;
                this.OnPropertyChanged();
            }
        }

        public TaskState State
        {
            get => this._state;
            protected set
            {
                this._state = value;
                DispatcherHelper.RunAsync(() => this.CAbortTask.RaiseCanExecuteChanged());
            }
        }

        public DateTime StartTimeStamp { get; protected set; }

        public TimeSpan Duration
        {
            get => this._duration;
            protected set
            {
                this._duration = value;
                this.OnPropertyChanged();
            }
        }

        public override bool IsActive => (this.State == TaskState.Running);

        public TaskResultState ResultState { get; protected set; } = TaskResultState.Unknown;

        public object Result { get; set; }

        public Thread TaskThread { get; protected set; }

        /// <summary>
        ///     User job argument
        /// </summary>
        public object ArgumentsObject { get; set; }

        #endregion
    }
}