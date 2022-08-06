using System;
using System.Threading;
using System.Threading.Tasks;

/// factory for coroutines
static class Actions {
    /// creates an action that can be invoked repeatedly, and only fires once delay
    /// seconds have elapsed since the last invocation
    public static Action Debounce(float delay, Action action) {
        var millis = (int)(delay * 1000.0f);

        // the current invocation, if any
        var curr = null as CancellationTokenSource;

        // create an action that cancels the previous task when invoked
        return async () => {
            // cancel prev task
            curr?.Cancel();
            curr = new CancellationTokenSource();

            // create a new delay
            var task = Task.Delay(millis, curr.Token);

            // if it completes, call the action
            await task;
            if (task.IsCompletedSuccessfully) {
                action();
            }
        };
    }
}