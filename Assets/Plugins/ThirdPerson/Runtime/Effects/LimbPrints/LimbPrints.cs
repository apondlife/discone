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
        var rig = c.Rig;

        for (var goal = AvatarIKGoal.LeftFoot; goal <= AvatarIKGoal.RightHand; goal += 1) {
            var evt = goal.AsStepEvent();
            if (!evts.Contains(evt)) {
                continue;
            }

            var limb = rig.FindLimb(goal);

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