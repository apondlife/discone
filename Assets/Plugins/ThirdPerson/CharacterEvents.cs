using System;
using System.Collections.Generic;

namespace ThirdPerson {

public sealed class CharacterEvents {
    // -- constants --
    static CharacterEvent k_First;
    static CharacterEvent k_Last;

    // -- props--
    CharacterState m_State;

    Dictionary<CharacterEvent, Action> m_Subscribers = new Dictionary<CharacterEvent, Action>();

    // -- lifetime --
    public CharacterEvents(CharacterState state) {
        m_State = state;
    }

    static CharacterEvents() {
        var evts = Enum.GetValues(typeof(CharacterEvent));

        // set constants
        k_First = (CharacterEvent)evts.GetValue(0);
        k_Last = (CharacterEvent)evts.GetValue(evts.Length - 1);
    }

    // -- commands --
    /// subscribe to a particular event
    public AnyDisposable Subscribe(CharacterEvent e, Action action) {
        m_Subscribers.TryAdd(e, null);
        m_Subscribers[e] += action;

        return new AnyDisposable(() => m_Subscribers[e] -= action);
    }

    internal void Schedule(CharacterEvent evt) {
        m_State.Curr.Events.Add(evt);
    }

    internal void DispatchAll() {
        var evts = m_State.Curr.Events;

        for (CharacterEvent evt = k_First; evt <= k_Last; evt = (CharacterEvent)((int)evt << 1)) {
            if (evts.Contains(evt) && m_Subscribers.TryGetValue(evt, out var actions)) {
                actions?.Invoke();
            }
        }
    }
}

}