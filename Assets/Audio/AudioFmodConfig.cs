using System;
using UnityAtoms.BaseAtoms;
using UnityEngine;

[CreateAssetMenu(menuName = "AudioFmodConfig")]
/// configure fmod in callback handler
public class AudioFmodConfig: FMODUnity.PlatformCallbackHandler {
    // -- refs --
    [Header("refs")]
    [Tooltip("if the game is running as a standalone server")]
    [SerializeField] BoolReference m_IsStandalone;

    // -- PlatformCallbackHandler --
    public override void PreInitialize(FMOD.Studio.System studioSystem, Action<FMOD.RESULT, string> reportResult) {
        // get system
        var res = studioSystem.getCoreSystem(out var system);
        reportResult(res, "studioSystem.getCoreSystem");
        if (res != FMOD.RESULT.OK) {
            return;
        }

        // check if standalone
        var isStandalone = m_IsStandalone.Value;
        #if UNITY_SERVER
        isStandalone = true;
        #elif !UNITY_EDITOR
        isStandalone = false;
        #endif

        // disable audio in standalone
        if (isStandalone) {
            res = system.setOutput(FMOD.OUTPUTTYPE.NOSOUND);
            reportResult(res, "coreSystem.setOutput(NOSOUND)");
            return;
        }
    }
}
