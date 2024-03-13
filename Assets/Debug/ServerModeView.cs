using UnityEngine;

namespace Discone {

[ExecuteAlways]
[RequireComponent(typeof(TMPro.TMP_Text))]
sealed class ServerModeView : MonoBehaviour
{
    #if UNITY_SERVER
    void Awake()
    {
        GetComponent<TMPro.TMP_Text>().enabled = true;
    }
    #endif
}

}