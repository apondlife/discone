using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarFlare : MonoBehaviour
{
    [SerializeField] Material m_MatchMaterial;
    [SerializeField] LensFlare m_LensFlare;

    private void OnValidate() {
        if(m_MatchMaterial == null) {
            m_MatchMaterial = GetComponent<Renderer>()?.sharedMaterial;
            if(m_MatchMaterial == null) {
                m_MatchMaterial = GetComponentInChildren<Renderer>()?.sharedMaterial;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        m_LensFlare.color = m_MatchMaterial.color;
    }
}
