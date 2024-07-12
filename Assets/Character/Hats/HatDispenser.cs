using Soil;
using UnityEngine;
using UnityAtoms.BaseAtoms;

public class HatDispenser : MonoBehaviour {
    [SerializeField] LayerMask m_CharacterLayer;
    [SerializeField] StringReference m_HatName;

    void OnTriggerEnter(Collider other) {
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