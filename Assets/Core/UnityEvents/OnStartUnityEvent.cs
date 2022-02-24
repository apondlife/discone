using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MutCommon
{
  public class OnStartUnityEvent : MonoBehaviour
  {
    [SerializeField] private UnityEvent callback;

    private void Start()
    {
      callback?.Invoke();
    }
  }
}