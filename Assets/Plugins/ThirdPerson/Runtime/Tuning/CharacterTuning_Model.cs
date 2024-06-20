using System;
using Soil;
using UnityEngine;

namespace ThirdPerson {

partial class CharacterTuning {
    [Tooltip("the tuning the character model")]
    public ModelTuning Model;

    /// the tuning for the character model
    [Serializable]
    public sealed class ModelTuning {
        [Header("tilt")]
        [Tooltip("the tilt angle as a fn of input")]
        public MapCurve Tilt_InputAngle;

        [Tooltip("the input tilt rotation speed in degrees / s")]
        public float Tilt_InputSpeed = 100.0f;
    }
}

}