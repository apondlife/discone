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

        // -- jumps --
        [Header("jumps")]
        [NonReorderable]
        [Tooltip("the tuning for each jump, sequentially")]
        public JumpTuning[] Jumps;

        [Serializable]
        public class JumpTuning {
            [Tooltip("the squash power as a fn of jump squat elapsed")]
            public AdsrCurve Squash;
        }

        // -- queries --
        /// get the jump tuning for the id
        public JumpTuning JumpById(JumpId id) {
            var n = Jumps.Length;
            if (n == 0) {
                return null;
            }

            if (id.Index >= n) {
                return Jumps[^1];
            }

            return Jumps[id.Index];
        }

        /// get the next jump tuning (to initiate a jump)
        public JumpTuning NextJump(CharacterState state) {
            return JumpById(state.Next.NextJump);
        }

        // -- distortion --
        [Header("distortion")]
        [Tooltip("a scale on intensity along the plane's axis")]
        public float Distortion_AxialScale;

        [Tooltip("a scale on intensity around the plane's axis (inversely proportional to axial)")]
        public float Distortion_RadialScale;

        [Tooltip("the stretch and squash intensity acceleration scale, 0 full squash, 1 no distortion, infinity infinitely stretched")]
        public FloatRange Distortion_Intensity_Acceleration;

        [Tooltip("the stretch and squash intensity velocity scale, 0 full squash, 1 no distortion, infinity infinitely stretched")]
        public FloatRange Distortion_Intensity_Velocity;

        [Tooltip("the movement distortion scale")]
        public float Distortion_Movement_A;

        [Tooltip("the exponentiated movement distortion scale")]
        public float Distortion_Movement_B;

        [Tooltip("the movement distortion exponent")]
        public float Distortion_Movement_K;

        [Tooltip("the intensity ease on acceleration based stretch & squash")]
        public DynamicEase.Config Distortion_Ease;

        // -- animation --
        [Header("animation")]
        [Tooltip("the duration of the default landing animation as a fn of inertia")]
        public MapCurve Animation_LandingDuration;
    }
}

}