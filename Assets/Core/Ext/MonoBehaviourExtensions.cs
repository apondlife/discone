using System;
using UnityEngine;

public static class MonoBehaviourExtensions
{
  public static void DoAfterTime(this MonoBehaviour obj, float time, Action action)
  {
    obj.StartCoroutine(CoroutineHelpers.DoAfterTimeCoroutine(time, action));
  }

  public static void DoNextFrame(this MonoBehaviour obj, Action action) => DoAfterFrames(obj, 1, action);

  public static void DoAfterFrames(this MonoBehaviour obj, int frames, Action action)
  {
    obj.StartCoroutine(CoroutineHelpers.SkipFramesCoroutine(frames, action));
  }
}