using UnityEngine;
using System;
using System.Collections;

/// factory for coroutines
static class Coroutines {
    /// a coroutine that runs an action at an interval
    public static IEnumerator Interval(float interval, Action action) {
        while (true) {
            yield return new WaitForSecondsRealtime(interval);
            action.Invoke();
        }
    }
}