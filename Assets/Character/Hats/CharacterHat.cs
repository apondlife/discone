using System.Collections.Generic;
using UnityEngine;

public class CharacterHat : MonoBehaviour {
    public const string k_NoHat = "none";
    [SerializeField] private string m_CurrentHat = k_NoHat;

    [SerializeField] private List<string> m_PossibleHats;
    [SerializeField] private Transform m_Head;

    // Start is called before the first frame update
    void Awake()
    {
        if(m_Head != null) {
            transform.SetParent(m_Head, true);
        }

        m_PossibleHats = new List<string>();
        foreach(Transform c in transform) {
            m_PossibleHats.Add(c.name);
        }
        SetHat(m_CurrentHat);
    }

    void Start() {
    }

    public void SetHat(string hatName) {
        if(hatName != k_NoHat && !m_PossibleHats.Contains(hatName)) {
            Debug.LogWarning($"no hat named {hatName}");
            return;
        }

        m_CurrentHat = hatName;
        foreach(Transform hat in transform) {
            hat.gameObject.SetActive(hat.name == hatName);
        }
    }
}
