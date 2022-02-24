using UnityEngine;
using UnityEngine.Events;

namespace MutCommon
{
  public class UnityEventInvoker : MonoBehaviour
  {
    [SerializeField] public UnityEvent UnityEvent;

    public bool isActive = true;
    public void Activate() => this.DoNextFrame(() => isActive = true);
    public void Deactivate() => this.DoNextFrame(() => isActive = false);
    public void Invoke()
    {
      if (!isActive) return;
      UnityEvent?.Invoke();
    }
  }
}