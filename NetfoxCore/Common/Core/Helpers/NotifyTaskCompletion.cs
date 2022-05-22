using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Netfox.Core.Helpers
{
    public class NotifyTaskCompletion<TResult> : NotifyTaskCompletion
    {
        private readonly Action _notifyPropertyChange;
        protected TResult DefaultResult = default(TResult);
        private readonly Func<Task<TResult>> _resultBody;

        public NotifyTaskCompletion(Task<TResult> task, bool @await = false) {
            this.RunTask(task, @await );
        }

        private void RunTask(Task<TResult> task, bool await)
        {
            this.Task = task;
            this.TaskLocal = task;
            if(@await) this.Await();
        }

        public void ReRun()
        {
            this.RunTask(this._resultBody(),false);
            this.TaskLocal.ContinueWith(t => { this._notifyPropertyChange(); });
        }

        public NotifyTaskCompletion(Task<TResult> task, Action notifyPropertyChange, bool await = false) : this(task, @await)
        {
            this._notifyPropertyChange = notifyPropertyChange;
            this.TaskLocal.ContinueWith(t => { notifyPropertyChange(); });
        }

        public NotifyTaskCompletion(Func<Task<TResult>> resultBody, Action notifyPropertyChange, bool await = false) : this(resultBody(), notifyPropertyChange, @await)
        {
            this._resultBody = resultBody;
        }

        public NotifyTaskCompletion(Func<Task<TResult>> resultBody, Action notifyPropertyChange, TResult @default, bool await = false)
            : this(resultBody, notifyPropertyChange, @await) { this.DefaultResult = @default; }

        public NotifyTaskCompletion(Func<Task<TResult>> resultBody, bool await = false) : this(resultBody(), @await) { }

        protected NotifyTaskCompletion() { }
        public Task<TResult> TaskLocal { get; set; }

        public new TResult Result
        {
            get
            {
                if(this.IsNotCompleted) this.Await();
                return (this.Status == TaskStatus.RanToCompletion)? this.TaskLocal.Result : this.DefaultResult;
            }
        }

        public new TaskAwaiter<TResult> GetAwaiter() { return this.WatchTaskAsync().GetAwaiter(); }

        public static implicit operator TResult(NotifyTaskCompletion<TResult> notifyTaskCompletion) { return notifyTaskCompletion.Result; }

        public static implicit operator Task<TResult>(NotifyTaskCompletion<TResult> notifyTaskCompletion) { return notifyTaskCompletion.TaskLocal; }

        private new async Task<TResult> WatchTaskAsync()
        {
            await base.WatchTaskAsync();
            return this.Result;
        }

        #region Overrides of Object
        public override string ToString() {return this.Result?.ToString() ?? base.ToString(); }
        #endregion
    }
    
    public class NotifyTaskCompletion : INotifyPropertyChanged
    {
        private bool _isTaskStateNotified;

        public NotifyTaskCompletion(Task task, bool await = false)
        {
            this.Task = task;
            if (@await)
                this.Await();
        }

        public NotifyTaskCompletion(Task task, Action notifyPropertyChange, bool await = false):this(task,@await)
        {
            this.Task.ContinueWith(t => { notifyPropertyChange(); });
        }

        protected NotifyTaskCompletion() { }

        public TaskStatus Status => this.Task?.Status ?? TaskStatus.Created;

        public bool IsCompleted => this.Task?.IsCompleted ?? false;

        public bool IsNotCompleted => !this.IsCompleted;

        public bool IsSuccessfullyCompleted => this.Status == TaskStatus.RanToCompletion;

        public bool IsCanceled => this.Task?.IsCanceled ?? false;

        public bool IsFaulted => this.Task?.IsFaulted ?? false;

        public AggregateException Exception => this.Task?.Exception;

        public Exception InnerException => this.Exception?.InnerException;

        public string ErrorMessage => this.InnerException?.Message;

        public string ErrorMessagesAll
        {
            get
            {
                var sb = new StringBuilder();
                this.InnerExceptionPrinter(this.Exception, sb);
                return sb.ToString();
            }
        }

        protected Task Task { get; set; }

        protected object Result => null;

        public event PropertyChangedEventHandler PropertyChanged;

        public TaskAwaiter GetAwaiter() { return this.Task.GetAwaiter(); }

        protected void Await()
        {
            if((this.IsNotCompleted))
            {
                var _ = this.WatchTaskAsync();
            }
        }

        protected async Task WatchTaskAsync()
        {
            try {
                await this.Task;
            }
            catch {}
            if(!this._isTaskStateNotified)
            {
                this._isTaskStateNotified = true;
                this.NotifyTaskStateChanged();
            }
        }

        private void InnerExceptionPrinter(Exception ex, StringBuilder sb)
        {
            if(ex == null) { return; }

            sb.AppendLine(ex.GetType().ToString());
            sb.AppendLine(ex.Message);

            this.InnerExceptionPrinter(ex.InnerException, sb);

            var readOnlyCollection = (ex as AggregateException)?.InnerExceptions;
            if(readOnlyCollection != null)
            {
                foreach(var innerException in readOnlyCollection) { this.InnerExceptionPrinter(innerException, sb); }
            }
        }

        private void NotifyTaskStateChanged()
        {
            var propertyChanged = this.PropertyChanged;
            if(propertyChanged == null) { return; }
            propertyChanged(this, new PropertyChangedEventArgs(nameof(this.Status)));
            propertyChanged(this, new PropertyChangedEventArgs(nameof(this.IsCompleted)));
            propertyChanged(this, new PropertyChangedEventArgs(nameof(this.IsNotCompleted)));
            if(this.Task.IsCanceled) {
                propertyChanged(this, new PropertyChangedEventArgs(nameof(this.IsCanceled)));
            }
            else if(this.Task.IsFaulted)
            {
                propertyChanged(this, new PropertyChangedEventArgs(nameof(this.IsFaulted)));
                propertyChanged(this, new PropertyChangedEventArgs(nameof(this.Exception)));
                propertyChanged(this, new PropertyChangedEventArgs(nameof(this.InnerException)));
                propertyChanged(this, new PropertyChangedEventArgs(nameof(this.ErrorMessage)));
                propertyChanged(this, new PropertyChangedEventArgs(nameof(this.ErrorMessagesAll)));
            }
            else
            {
                propertyChanged(this, new PropertyChangedEventArgs(nameof(this.IsSuccessfullyCompleted)));
                propertyChanged(this, new PropertyChangedEventArgs(nameof(this.Result)));
            }
        }
    }
}