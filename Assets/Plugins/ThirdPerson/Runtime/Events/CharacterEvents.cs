using System;
using System.Collections.Generic;

namespace ThirdPerson {

/// TODO: make this compatible with `DisposeBag`
public sealed class CharacterEvents {
    // -- constants --
    static readonly CharacterEvent s_First;
    static readonly CharacterEvent s_Last;

    // -- props--
    /// the character state
    readonly CharacterState m_State;

    /// a map of per-event subscriptions
    readonly Dictionary<CharacterEvent, Action> m_Subscribers = new();

    // -- lifetime --
    public CharacterEvents(CharacterState state) {
        m_State = state;
    }

    static CharacterEvents() {
        var evts = Enum.GetValues(typeof(CharacterEvent));

        // set constants
        s_First = (CharacterEvent)evts.GetValue(0);
        s_Last = (CharacterEvent)evts.GetValue(evts.Length - 1);
    }

    // -- commands --
    /// subscribe to a particular event
    public AnyDisposable Subscribe(CharacterEvent e, Action action) {
        m_Subscribers.TryAdd(e, null);
        m_Subscribers[e] += action;

        return new AnyDisposable(() => m_Subscribers[e] -= action);
    }

    /// subscribe to the next time the event happens
    public AnyDisposable Once(CharacterEvent e, Action action) {
        void OnceAction() {
            action();
            m_Subscribers[e] -= OnceAction;
        }

        return Subscribe(e, OnceAction);
    }

    /// schedule an event this frame
    internal void Schedule(CharacterEvent evt) {
        m_State.Next.Events.Add(evt);
    }

    /// dispatch events to the listeners
    internal void DispatchAll() {
        var evts = m_State.Next.Events;

        for (var evt = s_First; evt <= s_Last; evt = (CharacterEvent)((int)evt << 1)) {
            if (evts.Contains(evt) && m_Subscribers.TryGetValue(evt, out var actions)) {
                actions?.Invoke();
            }
        }
    }
}

}