using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ThirdPerson;
using UnityAtoms.BaseAtoms;

public class HatDispenser : MonoBehaviour
{
    [SerializeField] private LayerMask m_CharacterLayer;
    [SerializeField] private StringReference m_HatName;

    private void OnTriggerEnter(Collider other) {
        Debug.Log("ENTER" + other.name);
        if(!m_CharacterLayer.Contains(other.gameObject.layer)) {
            return;
        }

        var hat = other.transform.parent.GetComponent<OnlineHat>();
        if(hat == null) {
            return;
        }

        hat.GiveHat(m_HatName);
    }
}
