using UnityEngine;

namespace ThirdPerson {

/// the character's wall skid effect
sealed class CharacterWallSkid: MonoBehaviour {
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
        var isActive = c.State.Next.IsOnWall && !c.State.Next.IsIdle;

        // stop the particles
        if (!isActive) {
            if (m_Particles.isPlaying) {
                m_Particles.Stop();
            }

            return;
        }

        // or start them
        if (!m_Particles.isPlaying) {
            m_Particles.Play();
        }

        // spawn at collision
        var s = c.State.Next.WallSurface;
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
    }
}

}