using Musicker;
using ThirdPerson;
using UnityAtoms.BaseAtoms;
using UnityEngine;

public abstract class CharacterMusicBase: MonoBehaviour {
    /// if the music is audible
    bool m_IsAudible = true;

    // -- lifecycle --
    #if !UNITY_SERVER
    protected void Start() {
        SetIsAudible(false);
    }
    #endif

    // -- c/audibility
    /// toggles the music
    public void SetIsAudible(bool isAudible) {
        if (isAudible != m_IsAudible) {
            m_IsAudible = isAudible;
            gameObject.SetActive(isAudible);
        }
    }
}