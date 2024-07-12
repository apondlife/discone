using UnityEngine;

namespace ThirdPerson {

/// proxies animator events to another object
public class CharacterAnimatorProxy: MonoBehaviour {
    // -- props --
    /// a target for proxied animator events
    Target m_Target;

    // -- lifecycle --
    void OnAnimatorIK(int layer) {
        m_Target?.OnAnimatorIk(layer);
    }

    // -- commands --
    /// bind the proxied events to a target
    public void Bind(Target target) {
        m_Target = target;
    }

    // -- types --
    /// a target for proxied animator events
    public interface Target {
        /// an event for setting up animator ik
        void OnAnimatorIk(int layer);
    }
}

}