using UnityAtoms;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityAtoms.BaseAtoms;
using System;

namespace Discone {

// TODO: once / release create capturing lambdas; thus garbage. use attribute based binding & codegen?
// TODO: add once subscriptions
/// a collection of event subscriptions
public record DisposeBag: IDisposable {
    // -- props --
    /// a handle to release the resources
    Action m_Release;

    // -- commands --
    /// add a release action to the bag
    void Add(Action a) {
        m_Release += a;
    }

    // -- c/subscriptions
    /// add a subscription for an event/action pair
    public DisposeBag Add(VoidEvent e, Action a) {
        if (!e) {
            return this;
        }

        // it's important this use the Register(Action<T> a) and _not_ Register(Action a),
        // because the zero-arg version does not run the replay buffer.
        return Add(e, (_) => a.Invoke());
    }

    /// add a subscription for an event/action pair
    public DisposeBag Add<T>(AtomEvent<T> e, Action<T> a) {
        if (!e) {
            return this;
        }

        e.Register(a);
        Add(() => e.Unregister(a));
        return this;
    }

    /// add a subscription for an event/action pair
    public DisposeBag Add(UnityEvent e, UnityAction a) {
        if (e == null) {
            return this;
        }

        e.AddListener(a);
        Add(() => e.RemoveListener(a));
        return this;
    }

    /// add a subscription for an event/action pair
    public DisposeBag Add<T>(UnityEvent<T> e, UnityAction<T> a) {
        if (e == null) {
            return this;
        }

        e.AddListener(a);
        Add(() => e.RemoveListener(a));
        return this;
    }

    /// add a subscription for an input action/action pair
    public DisposeBag Add(InputAction e, Action<InputAction.CallbackContext> a) {
        if (e == null) {
            return this;
        }

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

}