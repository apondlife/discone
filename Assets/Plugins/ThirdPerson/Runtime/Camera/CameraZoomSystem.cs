using System;
using UnityEngine;

namespace ThirdPerson {

[Serializable]
sealed class CameraZoomSystem: CameraSystem {
    // -- System --
    protected override Phase InitInitialPhase() {
        return Zooming;
    }

    public override void Init() {
        base.Init();

        m_State.Fov = m_Tuning.Fov.Evaluate(0);
    }

    // -- Tracking --
    Phase Zooming => new Phase(
        name: "Zooming",
        update: Zooming_Update
    );

    void Zooming_Update(float delta) {
        var destFov = m_Tuning.Fov.Evaluate(
            Mathf.InverseLerp(
                m_Tuning.FovTargetMinSpeed,
                m_Tuning.FovTargetMaxSpeed,
                m_State.Character.Next.Velocity.magnitude
            )
        );

        m_State.Next.Fov = Mathf.MoveTowards(
            m_State.Fov,
            destFov,
            m_Tuning.FovSpeed * Time.deltaTime
        );
    }
}

}