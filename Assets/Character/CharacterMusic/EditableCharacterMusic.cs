using UnityEngine;
using Musicker;
using T = ThirdPerson;

/// the character's music

// TODO: better handling of stepInterval changing at runtime

class EditableCharacterMusic: MonoBehaviour {
    // -- tuning --
    [Header("tuning")]
    [Tooltip("the time interval between steps")]
    [SerializeField] float m_StepInterval = 0.2f;

    [Tooltip("the time interval between notes in the jump chord")]
    [SerializeField] float m_JumpInterval = 3.0f / 60.0f;

    // -- references --
    [Header("references")]
    [Tooltip("the footsteps music source")]
    [SerializeField] MusicSource m_Footsteps;

    [Tooltip("the jump music source")]
    [SerializeField] MusicSource m_Jump;

    [Tooltip("the character controller")]
    [SerializeField] T.ThirdPerson m_Controller;

    // -- music --
    [Header("music")]
    [Tooltip("the melody line when walking")]
    [SerializeField] Line m_FootstepsMelody =  new Line(
        Tone.I.Octave(),
        Tone.V.Octave()
    );
    // -- props --
    /// the current key root
    Root m_Root = Root.C;

    /// the musical key
    Key m_Key;

    /// the index of the current step
    int m_StepIdx;

    /// the index of the melody note to play
    int m_MelodyIdx;

    /// the current step time
    float m_StepTime = 0.0f;

    /// the time of the next step
    float m_NextStepTime = 0.0f;

    // -- lifecycle --
    void Awake() {
        // set props
        m_Key = new Key(m_Root);
    }

    void Update() {
        // update state
        Step();

        // play audio
        PlayStep();
        // PlayJump();
        // PlayFlutter();
    }

    // -- commands --
    // update current step progress
    void Step() {
        if (!m_Controller.IsGrounded) {
            return;
        }

        // copy a bunch of stuff from gpc
        float dist = StepVelocity.magnitude * Time.timeScale;
        float stride = 1.0f + dist * 0.3f;
        m_StepTime += (dist / stride) * (Time.deltaTime / m_StepInterval);
    }

    // -- c/play
    /// play step audio
    void PlayStep() {
        if (m_Footsteps == null) {
            return;
        }

        // if were stepping at all
        if (StepVelocity == Vector3.zero) {
            return;
        }

        // if it's time to play a step
        if (m_StepTime < m_NextStepTime) {
            return;
        }

        // find line to play
        m_Footsteps.PlayTone(m_FootstepsMelody[m_StepIdx], m_Key);
        
        // advance step
        m_StepIdx = (m_StepIdx + 1) % m_FootstepsMelody.Length;
        m_NextStepTime += 0.5f;
    }

    // -- queries --
    /// the character's step (planar) velocity
    Vector3 StepVelocity {
        get => m_Controller.PlanarVelocity;
    }
}