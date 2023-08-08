using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// For passing step events to the character music generator
// (This maybe should be merged with CharacterAnimatorProxy, but seems like that is unused right now so idk)
public class CharacterAnimatorEventReceiver : MonoBehaviour
{
    public CharacterMusicBase music;
    // Note that OnWalkStep and OnRunStep sometimes get called simultaneously, because the walk and run animations are blended together
    public void OnWalkStep(int foot) {
        if (music) music.OnStep(foot, false);
    }
    public void OnRunStep(int foot) {
        if (music) music.OnStep(foot, true);
    }
}
