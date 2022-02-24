using System;
using System.Collections;
using UnityEngine;

namespace MutCommon
{
  public static class CoroutineHelpers
  {
    public static IEnumerator InterpolateByTime(float time, System.Action<float> interpolator, Action callback = null)
      => InterpolateByTimeCustom(() => Time.deltaTime, time, interpolator, callback);

    public static IEnumerator InterpolateByTimeFixed(float time, System.Action<float> interpolator, Action callback = null)
      => InterpolateByTimeCustom(() => Time.fixedDeltaTime, time, interpolator, callback);

    public static IEnumerator InterpolateByUnscaledTimeFixed(float time, System.Action<float> interpolator, Action callback = null)
      => InterpolateByTimeCustom(() => Time.fixedUnscaledDeltaTime, time, interpolator, callback);

    public static IEnumerator InterpolateByUnscaledTime(float time, System.Action<float> interpolator, Action callback = null)
      => InterpolateByTimeCustom(() => Time.unscaledDeltaTime, time, interpolator, callback);

    public static IEnumerator InterpolateByTimeCustom(Func<float> deltaTimeGetter, float time, System.Action<float> interpolator, Action callback = null)
    {
      for (float t = 0f; t < time; t += deltaTimeGetter())
      {
        var k = t / time;
        interpolator(k);
        yield return null;
      }
      interpolator(1);
      callback?.Invoke();
    }

    public static IEnumerator DoAfterTimeCoroutine(float time, Action action)
    {
      yield return new WaitForSeconds(time);

      action();
    }

    public static IEnumerator DoAfterRealtimeTimeCoroutine(float time, Action action)
    {
      yield return new WaitForSecondsRealtime(time);

      action();
    }

    public static IEnumerator SkipFramesCoroutine(int frames, Action action)
    {
      for (int i = 0; i < frames; i++)
        yield return null;

      action();
    }
  }
}