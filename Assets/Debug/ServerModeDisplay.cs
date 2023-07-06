using UnityEngine;
using UnityAtoms.BaseAtoms;

namespace Discone {

[RequireComponent(typeof(TMPro.TMP_Text))]
public class ServerModeDisplay : MonoBehaviour
{
    [Tooltip("if this object should display or not")]
    [SerializeField] BoolReference m_ShowText;

    void Awake()
    {
        GetComponent<TMPro.TMP_Text>().enabled = m_ShowText;
    }
}

}