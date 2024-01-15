using UnityAtoms;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityAtoms.BaseAtoms;
using System;

/// a collection of event subscriptions
public record DisposeBag: IDisposable {
    // -- props --
    /// a handle to release the resources
    Action m_Release;

    // -- commands --
    /// add a release action to the bag
    public void Add(Action a) {
        m_Release += a;
    }

    // -- c/subscriptions
    /// add a subscription for an event/action pair
    public DisposeBag Add(VoidEvent e, Action a) {
        Add(e, (_) => a.Invoke());
        return this;
    }

    /// add a subscription for an event/action pair
    public DisposeBag Add<T>(AtomEvent<T> e, Action<T> a) {
        e.Register(a);
        Add(() => e.Unregister(a));
        return this;
    }

    /// add a subscription for an event/action pair
    public DisposeBag Add(UnityEvent e, UnityAction a) {
        e.AddListener(a);
        Add(() => e.RemoveListener(a));
        return this;
    }

    /// add a subscription for an event/action pair
    public DisposeBag Add<T>(UnityEvent<T> e, UnityAction<T> a) {
        e.AddListener(a);
        Add(() => e.RemoveListener(a));
        return this;
    }

    /// add a subscription for an input action/action pair
    public DisposeBag Add(InputAction e, Action<InputAction.CallbackContext> a) {
        e.performed += a;
        Add(() => e.performed -= a);
        return this;
    }

    // -- IDisposable --
    /// dispose of all the subscriptions
    public void Dispose() {
        m_Release?.Invoke();
        m_Release = null;
    }
}