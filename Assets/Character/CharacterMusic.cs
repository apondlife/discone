using UnityEngine;
using Musicker;
using ThirdPerson;
using System;

/// the character's music ("m***** mousing")
class CharacterMusic: MonoBehaviour {
    // -- tuning --
    [Header("tuning")]
    [Tooltip("the time interval between steps")]
    [SerializeField] float m_StepInterval = 0.2f;

    [Tooltip("the time interval between notes in the jump chord")]
    [SerializeField] float m_JumpInterval = 3.0f / 60.0f;

    // -- music --
    [Header("music")]
    [Tooltip("the bass line when walking")]
    [SerializeField] LineField m_FootstepsBass;

    [Tooltip("the melody line when walking")]
    [SerializeField] LineField[] m_FootstepsMelodies;

    [Tooltip("the progression when jumping")]
    [SerializeField] ProgressionField m_Jump;

    [Tooltip("the line to play when fluttering")]
    [SerializeField] LineField m_Flutter;

    // -- refs --
    [Header("refs")]
    [Tooltip("the music source")]
    [SerializeField] FmodMusicSource m_Source;

    // -- props --
    /// the containing DisconeCharacter
    // TODO: inject this better in the future (parent call these events)
    DisconeCharacter m_Container;

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

    /// the time to start fluttering
    float m_FlutterTime = 0.0f;

    // -- lifecycle --
    void Start() {
        // set deps
        m_Container = GetComponentInParent<DisconeCharacter>();

        //  set events
        m_Container.Character.Events.Bind(CharacterEvent.Jump, PlayJump);
        m_Container.OnSimulationChanged += OnSimulationChanged;

        // set props
        m_Key = new Key(m_Root);
    }

    void Update() {
        // update state
        Step();
        Flutter();

        // play audio
        PlayStep();
        PlayFlutter();
    }

    // -- commands --
    // update current step progress
    void Step() {
        if (!State.IsGrounded) {
            return;
        }

        // determine note based on velocity
        var v = StepVelocity;

        // pick melody note based on move dir
        var dirW = Vector3.Dot(Vector3.Normalize(v), transform.forward);
        m_MelodyIdx = dirW switch {
            var d when d > +0.8f => 0,
            var d when d > +0.3f => 1,
            var d when d > -0.3f => 2,
            var d when d > -0.8f => 3,
            _                    => 4,
        };

        // pick key based on look dir
        var dirL = Vector3.Dot(transform.forward, Vector3.forward);
        var root = dirL switch {
            var d when d > +0.8f => Root.C,
            var d when d > +0.3f => Root.G,
            var d when d > -0.3f => Root.D,
            var d when d > -0.8f => Root.A,
            _                    => Root.E,
        };

        if (m_Root != root) {
            m_Root = root;
            m_Key = new Key(m_Root);
        }

        // copy a bunch of stuff from gpc
        float dist = v.magnitude * Time.timeScale;
        float stride = 1.0f + dist * 0.3f;
        m_StepTime += (dist / stride) * (Time.deltaTime / m_StepInterval);
    }

    /// flutter when airborne
    void Flutter() {
        if (State.IsGrounded) {
            m_FlutterTime = -1.0f;
            return;
        }

        if (m_FlutterTime == -1.0f) {
            m_FlutterTime = Time.time + 0.5f;
        }
    }

    // -- c/play
    /// play step audio
    void PlayStep() {
        // if were stepping at all
        if (StepVelocity == Vector3.zero) {
            return;
        }

        // if it's time to play a step
        if (m_StepTime < m_NextStepTime) {
            return;
        }

        // find line to play
        if (m_StepIdx % 2 == 0) {
            m_Source.PlayLine(m_FootstepsBass.Value, 0.0f, m_Key);
        } else {
            var melody = m_FootstepsMelodies[m_MelodyIdx];
            m_Source.PlayNote(melody.Value[m_StepIdx / 2], 0.0f, m_Key);
        }

        // advance step
        m_StepIdx = (m_StepIdx + 1) % 4;
        m_NextStepTime += 0.5f;
    }

    /// play jump audio
    void PlayJump() {
        m_Source.PlayProgression(
            m_Jump.Value,
            m_JumpInterval,
            m_Key
        );
    }

    /// play flutter audio
    void PlayFlutter() {
        if (m_FlutterTime == -1.0f) {
            return;
        }

        if (Time.time < m_FlutterTime) {
            return;
        }

        m_Source.PlayLine(m_Flutter.Value, 0.0f, m_Key);
        m_FlutterTime += 0.1f;
    }

    // -- events --
    private void OnSimulationChanged(DisconeCharacter.Simulation sim)
    {
        enabled = sim != DisconeCharacter.Simulation.None;
    }

    // -- queries --
    /// the character's step (planar) velocity
    Vector3 StepVelocity {
        get => State.Curr.GroundVelocity;
    }

    ThirdPerson.CharacterState State {
        get => m_Container.Character.State;
    }
}