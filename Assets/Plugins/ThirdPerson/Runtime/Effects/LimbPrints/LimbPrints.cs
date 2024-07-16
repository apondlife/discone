using UnityEngine;

namespace ThirdPerson {

/// marks left by the characters hands, feet, &c
public class LimbPrints: MonoBehaviour {
    // -- refs --
    [Header("refs")]
    [Tooltip("the particle system")]
    [SerializeField] ParticleSystem m_Particles;

    // -- props --
    /// the containing character
    CharacterContainer c;

    // -- lifecycle --
    void Start() {
        // set deps
        c = GetComponentInParent<CharacterContainer>();
    }

    void FixedUpdate() {
        // TODO: for some reason this script runs before the character
        // so we use Curr here instead of Next
        var evts = c.State.Curr.Events;
        if (!evts.Contains(CharacterEvent.Step)) {
            return;
        }

        foreach (var limb in c.Rig.Limbs) {
            if (!limb.MatchesStep(evts.Mask)) {
                continue;
            }

            var placement = limb.Placement;

            // move the emitter
            var trs = m_Particles.transform;
            trs.position = placement.Pos;

            // point towards the current surface
            var normal = placement.Normal;
            normal.z = -normal.z;

            var rot = Quaternion.LookRotation(normal);

            // update the start rotation
            var main = m_Particles.main;
            var a = rot.eulerAngles * Mathf.Deg2Rad;
            main.startRotationX = a.x;
            main.startRotationY = a.y;
            main.startRotationZ = a.z;

            m_Particles.Emit(1);
        }
    }
}

}