using System;
using UnityEngine;

namespace Soil {

/// the state for a system
[Serializable]
public struct SystemState: IEquatable<SystemState> {
    // -- fields --
    [Tooltip("the current phase")]
    public string PhaseName;

    [Tooltip("the current phase start time")]
    public float PhaseStart;

    [Tooltip("the time in the current phase")]
    public float PhaseElapsed;

    // -- IEquatable --
    public override bool Equals(object o) {
        if (o is SystemState c) {
            return Equals(c);
        } else {
            return false;
        }
    }

    public bool Equals(SystemState o) {
        return (
            PhaseName == o.PhaseName &&
            PhaseStart == o.PhaseStart &&
            PhaseElapsed == o.PhaseElapsed
        );
    }

    public override int GetHashCode() {
        return HashCode.Combine(PhaseName, PhaseStart, PhaseElapsed);
    }

    public static bool operator ==(
        SystemState a,
        SystemState b
    ) {
        return a.Equals(b);
    }

    public static bool operator !=(
        SystemState a,
        SystemState b
    ) {
        return !(a == b);
    }
}

}