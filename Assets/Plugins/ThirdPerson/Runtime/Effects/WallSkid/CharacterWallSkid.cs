using UnityEngine;

namespace ThirdPerson {

/// the character's wall skid effect
sealed class CharacterWallSkid: MonoBehaviour {
    // -- tuning --
    [Header("tuning")]
    [Tooltip("the number of particles per frame")]
    [SerializeField] int m_ParticleCount;

    // -- refs --
    [Header("refs")]
    [Tooltip("the wall particle emitter")]
    [SerializeField] ParticleSystem m_Particles;

    // -- props --
    /// the character container
    CharacterContainer c;

    // -- lifecycle --
    void Start() {
        // set container
        this.c = GetComponentInParent<CharacterContainer>();
    }

    void FixedUpdate() {
        // only play particles when wall & not idle
        if (!c.State.Next.IsOnWall || c.State.Next.IsIdle) {
            return;
        }

        // spawn at collision
        var s = c.State.Next.MainSurface;
        var t = m_Particles.transform;
        t.position = s.Point;

        // use inverted z-axis bc particle systems want that
        var n = s.Normal;
        n.z = -n.z;

        // point towards the current surface
        var rot = Quaternion.LookRotation(n);

        // and rotate along the normal axis
        rot *= Quaternion.AngleAxis(Random.Range(0, 360), Vector3.forward);

        // update the start rotation
        var main = m_Particles.main;
        var a = rot.eulerAngles * Mathf.Deg2Rad;
        main.startRotationX = a.x;
        main.startRotationY = a.y;
        main.startRotationZ = a.z;

        // emit particles
        m_Particles.Emit(m_ParticleCount);
    }
}

}