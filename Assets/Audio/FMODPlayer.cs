using System;
using System.Collections.Generic;
using UnityEngine;

using FMODUnity;
public class FMODParams : Dictionary<string, float> {}

public struct FMODEvent {
    public StudioEventEmitter emitter;
    public FMODParams parameters;
    public FMODEvent(StudioEventEmitter emitter, FMODParams parameters) {
        this.emitter = emitter;
        this.parameters = parameters;
    }
}

// [not sure this should be a static class]
public static class FMODPlayer {
    public static void PlayEvent(FMODEvent e) {
        // play the event for this note
        if (!e.emitter) {
            Debug.LogWarning("FMODPlayer.PlayEvent received FMODEvent with no emitter");
            return;
        }

        e.emitter.Play();
        
        if (e.parameters != null) {
            foreach ((string name, float val) in e.parameters) {
                e.emitter.SetParameter(name, val);
            }
        }
    }

    public static void PlayAll(IEnumerable<FMODEvent> es) {
        foreach (FMODEvent e in es) {
            PlayEvent(e);
        }
    }
}