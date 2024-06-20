using System;
using Soil;
using UnityEngine;
using UnityEngine.Serialization;

namespace ThirdPerson {

partial class CharacterTuning {
    [Tooltip("the tuning the character model")]
    public ModelTuning Model;

    /// the tuning for the character model
    [Serializable]
    public sealed class ModelTuning {
        [Header("tilt")]
        [Tooltip("the tilt angle as a fn of acceleration")]
        public MapCurve Tilt_AccelerationAngle;

        [Tooltip("the tilt angle as a fn of input magnitude")]
        public MapOutCurve Tilt_InputAngle;

        // TODO: this should be a dynamic ease, but since that is stateful it can't be stored
        // in the tuning (like `EaseTimer`). need to extract a config sub-object.
        [Tooltip("the movement tilt (accel * input) rotation speed in degrees / s")]
        public float Tilt_MoveSpeed = 100.0f;

        [Tooltip("the tilt angle as a fn of surface angle")]
        public MapOutCurve Tilt_SurfaceAngle;

        [Tooltip("the surface tilt rotation speed in degrees / s")]
        public float Tilt_SurfaceSpeed = 100.0f;
    }
}

}