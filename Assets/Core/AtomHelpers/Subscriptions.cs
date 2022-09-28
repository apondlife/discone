using UnityAtoms;
using UnityAtoms.BaseAtoms;
using System;

/// a collection of event subscriptions
sealed class Subscriptions: IDisposable {
    // -- props --
    private Action subscriptions;

    // TODO: make this work for non atoms

    // -- commands --
    /// add a subscription for an event/action pair
    public Subscriptions Add(VoidEvent e, Action a) {
        Add<UnityAtoms.Void>(e, (_) => a.Invoke());
        return this;
    }

    /// add a subscription for an event/action pair
    public Subscriptions Add<T>(AtomEvent<T> e, Action<T> a) {
        e.Register(a);
        subscriptions += () => e.Unregister(a);
        return this;
    }

    /// dispose of all the subscriptions
    public void Dispose() {
        subscriptions?.Invoke();
        subscriptions = null;
    }
}