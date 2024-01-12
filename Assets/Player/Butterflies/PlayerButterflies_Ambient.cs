using UnityEngine;
using UnityEngine.Events;

namespace Discone {

/// the ambient butterflies around the player
[RequireComponent(typeof(ParticleSystem))]
sealed class PlayerButterflies_Ambient: MonoBehaviour {
    // -- props --
    /// an event when the player collects butterflies
    public UnityEvent OnCollectTrigger;

    // -- events --
    /// when the butterflies hit the character
    void OnParticleTrigger() {
        OnCollectTrigger?.Invoke();
    }
}

}