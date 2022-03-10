using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

namespace MutCommon
{
  public class InputActionReferenceUnityEvent : MonoBehaviour
  {
    public enum KeyCodeType
    {
      Down,
      Hold,
      Up,
    }

    [Serializable]
    public class KeyCodeTypeEvent
    {
      public KeyCode Key;
      public KeyCodeType Type;
      public UnityEvent Event;
    }

    [SerializeField] private InputActionReference InputAction;
    [SerializeField] public KeyCodeType Type;
    [SerializeField] public UnityEvent Event;

    void Update()
    {
      switch (Type)
      {
        case KeyCodeType.Down:
          if (InputAction.action.WasPressedThisFrame()) Event.Invoke();
          break;
        case KeyCodeType.Hold:
          if (InputAction.action.IsPressed()) Event.Invoke();
          break;
        case KeyCodeType.Up:
          if (InputAction.action.WasReleasedThisFrame()) Event.Invoke();
          break;
      }
    }
  }
}