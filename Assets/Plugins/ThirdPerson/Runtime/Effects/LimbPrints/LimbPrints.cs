using UnityEngine;

namespace ThirdPerson {

/// marks left by the characters hands, feet, &c
public class LimbPrints: CharacterBehaviour {
    // -- refs --
    [Header("refs")]
    [Tooltip("the particle system")]
    [SerializeField] ParticleSystem m_Particles;

    // -- lifecycle --
    public override void Step_Fixed_I(float delta) {
        base.Step_Fixed_I(delta);

        var next = c.State.Next;
        var evts = next.Events;

        for (var goal = AvatarIKGoal.LeftFoot; goal <= AvatarIKGoal.RightHand; goal += 1) {
            var evt = goal.AsStepEvent();
            if (!evts.Contains(evt) ) {
                continue;
            }

            var placement = c.Rig.FindLimb(goal).Placement;

            // move the emitter
            var trs = m_Particles.transform;
            trs.position = placement.Pos;

            // point towards the current surface
            // TODO: placement.Forward
            var rot = Quaternion.LookRotation(-placement.Normal, next.Forward);

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