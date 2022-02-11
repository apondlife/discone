using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MutCommon
{
  public class OnAwakeUnityEvent : MonoBehaviour
  {
    [SerializeField] private UnityEvent callback;

    private void Awake()
    {
      callback?.Invoke();
    }
  }
}