using System;
using UnityEngine;

namespace ThirdPerson {

/// the state for a system
[Serializable]
public record SystemState {
    [Tooltip("the current phase")]
    public string PhaseName;

    [Tooltip("the current phase start time")]
    public float PhaseStart;

    [Tooltip("the time in the current phase")]
    public float PhaseElapsed;
}

}