using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ThirdPerson;
using System.Linq;
using System;
using UnityAtoms.BaseAtoms;

public class CycleCharacters : MonoBehaviour
{
    private int current = 0;
    [SerializeField] GameObjectEvent m_SwitchCharacter;
    // Start is called before the first frame update

    public void CycleAvailable() {
        CycleList(c => c.IsAvailable);
    }

    public void CycleStarters() {
        CycleList(c => c.IsAvailable && c.IsInitial);
    }

    private void CycleList(Func<OnlineCharacter, bool> filter) {
        var characters = FindObjectsOfType<OnlineCharacter>().Where(filter);
        var player = GetComponentInParent<DisconePlayer>();
        current = (current + 1) % characters.Count();
        m_SwitchCharacter?.Raise(characters.ElementAt(current).gameObject);
    }
}
