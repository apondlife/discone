using UnityEngine;

public abstract class CharacterMusicBase: MonoBehaviour {
    // -- props --
    /// the containing DisconeCharacter
    // TODO: inject this better in the future (parent call these events)
    protected DisconeCharacter m_Container;

    /// if the music is audible
    bool m_IsAudible = false;

    // -- lifecycle --
    #if !UNITY_SERVER
    protected virtual void Start() {
        // set deps
        m_Container = GetComponentInParent<DisconeCharacter>();
    }
    #endif

    public virtual void OnStep(int foot, bool isRunning) {}

    // -- c/audibility
    /// toggles the music
    public void SetIsAudible(bool isAudible) {
        if (isAudible != m_IsAudible) {
            m_IsAudible = isAudible;
            gameObject.SetActive(isAudible);
        }
    }

    protected virtual FMODParams CurrentFmodParams {
        get => new FMODParams { };
    }

    protected ThirdPerson.CharacterState State {
        get => m_Container?.Character?.State;
    }

    public bool HasCharacter {
        get => State != null;
    }
}