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
        return () => {
            // cancel prev task
            curr?.Cancel();
            curr = new CancellationTokenSource();

            // start the new task (_ is only to avoid weird 4 char alignment)
            var _ = Task
                .Delay(millis, curr.Token)
                .ContinueWith(
                    (t) => {
                        if (t.IsCompletedSuccessfully) {
                            action();
                        }
                    },
                    TaskScheduler.Default
                );
        };
    }
}