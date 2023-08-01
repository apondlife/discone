using UnityEngine;
using UnityAtoms.BaseAtoms;

namespace Discone {

[RequireComponent(typeof(TMPro.TMP_Text))]
public class ServerModeDisplay : MonoBehaviour
{
    #if UNITY_SERVER
    void Awake()
    {
        GetComponent<TMPro.TMP_Text>().enabled = true;
    }
    #endif
}

}