using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class OnlineHat : NetworkBehaviour
{
    [SyncVar(hook = nameof(SetHat))]
    [SerializeField] private string m_CurrentHat = CharacterHat.k_NoHat;
    private CharacterHat m_Hat;

    // Start is called before the first frame update
    void Awake()
    {
        m_Hat = GetComponentInChildren<CharacterHat>();
    }

    public void GiveHat(string hatName) {
        m_CurrentHat = hatName;
    }

    void SetHat(string oldHatName, string newHatName) {
        m_Hat.SetHat(newHatName);
    }
}
