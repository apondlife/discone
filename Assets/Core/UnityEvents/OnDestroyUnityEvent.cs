using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MutCommon
{
  public class OnDestroyUnityEvent : MonoBehaviour
  {
    [SerializeField] public UnityEvent callback;

    private void OnDestroy()
    {
      callback?.Invoke();
    }
  }
}