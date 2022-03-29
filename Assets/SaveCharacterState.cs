using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityAtoms.BaseAtoms;
using ThirdPerson;

public class SaveCharacterState : MonoBehaviour
{
    public GameObjectVariable currentCharacter;

    private CharacterState.Frame m_SavedState;

    // Start is called before the first frame update
    public void Start() {
        //currentCharacter.Changed
    }

    // Update is called once per frame
    public void LoadState() {
        Debug.Log("LOADING STATE");
        currentCharacter.GetComponent<Character>().ForceState(m_SavedState);
    }

    public void SaveState()
    {
        Debug.Log("Saving STATE");
        m_SavedState = new CharacterState.Frame(currentCharacter.GetComponent<Character>().State.GetFrame(1));
    }
}
