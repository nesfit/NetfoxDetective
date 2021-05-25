using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Netfox.Core.Helpers
{
    /// <summary>
    /// http://stackoverflow.com/questions/13695499/proper-way-to-implement-a-never-ending-task-timers-vs-task
    /// </summary>
    public static class PeriodicalRepetitiveTask
    {
        public static ActionBlock<DateTimeOffset> Create(Action<DateTimeOffset> action, CancellationToken cancellationToken, TimeSpan delay)
        {
            // Validate parameters.
            if(action == null) throw new ArgumentNullException(nameof(action));

            // Declare the block variable, it needs to be captured.
            ActionBlock<DateTimeOffset> block = null;

            // Create the block, it will call itself, so
            // you need to separate the declaration and
            // the assignment.
            // Async so you can wait easily when the
            // delay comes.
            block = new ActionBlock<DateTimeOffset>(async now =>
            {
                // Perform the action.
                action(now);

                // Wait.
                await Task.Delay(delay, cancellationToken).
                    // Doing this here because synchronization context more than
                    // likely *doesn't* need to be captured for the continuation
                    // here.  As a matter of fact, that would be downright
                    // dangerous.
                    ConfigureAwait(false);

                // Post the action back to the block.
                block.Post(DateTimeOffset.Now);
            }, new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken
            });

            // Return the block.
            return block;
        }

        public static ActionBlock<DateTimeOffset> Create(Func<DateTimeOffset, CancellationToken, Task> action, CancellationToken cancellationToken, TimeSpan delay)
        {
            // Validate parameters.
            if(action == null) throw new ArgumentNullException(nameof(action));

            // Declare the block variable, it needs to be captured.
            ActionBlock<DateTimeOffset> block = null;

            // Create the block, it will call itself, so
            // you need to separate the declaration and
            // the assignment.
            // Async so you can wait easily when the
            // delay comes.
            block = new ActionBlock<DateTimeOffset>(async now =>
            {
                // Perform the action.  Wait on the result.
                await action(now, cancellationToken).
                    // Doing this here because synchronization context more than
                    // likely *doesn't* need to be captured for the continuation
                    // here.  As a matter of fact, that would be downright
                    // dangerous.
                    ConfigureAwait(false);

                // Wait.
                try
                {
                    await Task.Delay(delay, cancellationToken)
                        .
                        // Same as above.
                        ConfigureAwait(false);
                }
                catch (OperationCanceledException) {}

                // Post the action back to the block.
                block.Post(DateTimeOffset.Now);
            }, new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken
            });

            // Return the block.
            return block;
        }
    }
}