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
    public void Awake() {
        c = GetComponentInParent<CharacterContainer>();
    }

    public void FixedUpdate() {
        var state = c.State.Curr;

        for (var goal = AvatarIKGoal.LeftFoot; goal <= AvatarIKGoal.RightHand; goal += 1) {
            if (!state.Events.Contains(goal.AsStepEvent()) ) {
                continue;
            }

            var placement = c.Rig.FindLimb(goal).Placement;

            // move the emitter
            var trs = m_Particles.transform;
            trs.position = placement.Pos;

            // point towards the current surface
            // TODO: placement.Forward
            var rot = Quaternion.LookRotation(-placement.Normal, state.Forward);

            // update the start rotation
            var main = m_Particles.main;

            // makes the particle oriented towards +z, instead of the -z default
            main.flipRotation = 1f;

            // apply the rotation
            var a = rot.eulerAngles * Mathf.Deg2Rad;
            main.startRotationX = a.x;
            main.startRotationY = a.y;
            main.startRotationZ = a.z;

            m_Particles.Emit(1);
        }
    }
}

}