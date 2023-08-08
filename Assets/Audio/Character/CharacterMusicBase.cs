using Musicker;
using ThirdPerson;
using UnityAtoms.BaseAtoms;
using UnityEngine;

public abstract class CharacterMusicBase: MonoBehaviour {
    // -- props --
    /// the containing DisconeCharacter
    // TODO: inject this better in the future (parent call these events)
    protected DisconeCharacter m_Container;

    /// if the music is audible
    bool m_IsAudible = true;

    public static readonly string k_ParamGrounded = "IsGrounded";

    // -- lifecycle --
    #if !UNITY_SERVER
    protected virtual void Start() {
        // set deps
        m_Container = GetComponentInParent<DisconeCharacter>();

        //  set events
        m_Container.OnSimulationChanged += OnSimulationChanged;
    }
    #endif

    // -- events --
    private void OnSimulationChanged(DisconeCharacter.Simulation sim)
    {
        enabled = sim != DisconeCharacter.Simulation.None;
    }

    public virtual void OnStep(int foot, bool isRunning) {}

    // -- c/audibility
    /// toggles the music
    public void SetIsAudible(bool isAudible) {
        if (isAudible != m_IsAudible) {
            m_IsAudible = isAudible;
            gameObject.SetActive(isAudible);
        }
    }

    protected virtual FMODParams CurrentFmodParams => new FMODParams {
        [k_ParamGrounded] = State.Next.IsOnGround ? 1f : 0f
    };

    protected ThirdPerson.CharacterState State => m_Container.Character.State;
}