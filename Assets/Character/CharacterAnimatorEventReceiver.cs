using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

// For passing step events to the character music generator
// (This maybe should be merged with CharacterAnimatorProxy, but seems like that is unused right now so idk)
public class CharacterAnimatorEventReceiver : MonoBehaviour
{
    [FormerlySerializedAs("music")]
    [SerializeField] CharacterMusicBase m_CharacterMusic;

    // Note that OnWalkStep and OnRunStep sometimes get called simultaneously, because the walk and run animations are blended together
    public void OnWalkStep(int foot) {
        if (m_CharacterMusic && m_CharacterMusic.HasCharacter) {
            m_CharacterMusic.OnStep(foot, false);
        }
    }
    public void OnRunStep(int foot) {
        if (m_CharacterMusic && m_CharacterMusic.HasCharacter) {
            m_CharacterMusic.OnStep(foot, true);
        }
    }
}