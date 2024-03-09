using Soil;
using UnityEngine;

namespace ThirdPerson {

[CreateAssetMenu(fileName = "CameraTuning", menuName = "thirdperson/CameraTuning", order = 1)]
public sealed class CameraTuning: ScriptableObject {
    // -- axis --
    [Header("axis")]
    [Tooltip("if the x-axis is inverted")]
    public bool IsInvertedX;

    [Tooltip("if the y-axis is inverted")]
    public bool IsInvertedY;

    // -- movement --
    [Header("movement")]
    [Tooltip("the fixed distance from the target")]
    public float MinRadius;

    [Tooltip("how much the camera yaws around the character as a fn of angle")]
    public AnimationCurve Tracking_YawCurve;

    [Tooltip("the max speed the camera yaws around the character")]
    public float Tracking_YawSpeed;

    [Tooltip("the acceleration of the camera yaw")]
    public float YawAcceleration;

    [Tooltip("the minimum angle the camera rotates around the character vertically")]
    public float Tracking_MinPitch;

    [Tooltip("the maximum angle the camera rotates around the character vertically")]
    public float Tracking_MaxPitch;

    [Tooltip("the maximum pitch speed")]
    public float Tracking_PitchSpeed;

    [Tooltip("the acceleration of the camera pitch")]
    public float Tracking_PitchAcceleration;

    [Tooltip("the number of frames without input to move to idle")]
    public int Tracking_IdleFrames;

    // -- target speed --
    [Header("target speed")]
    [Tooltip("the speed the camera distance adjusts in freelook")]
    public float DollySpeed;

    [Tooltip("the camera distance multiplier as a function of target speed")]
    public AnimationCurve DollySpeedCurve;

    [Tooltip("the minimum speed for curving camera distance")]
    public float DollyTargetMinSpeed;

    [Tooltip("the maximum speed for curving camera distance")]
    public float DollyTargetMaxSpeed;

    [Tooltip("the maximum speed to camera distance")]
    public float MaxRadius;

    [Tooltip("the camera's field of view as a function of target speed")]
    public MapOutCurve Fov;

    [Tooltip("the minimum speed for curving camera distance")]
    public float FovTargetMinSpeed;

    [Tooltip("the maximum speed for curving camera distance")]
    public float FovTargetMaxSpeed;

    [Tooltip("the rate of change for the fov")]
    public float FovSpeed;

    // -- freelook --
    [Header("free look")]
    [Tooltip("the speed the free look camera yaws")]
    public float FreeLook_YawSpeed;

    [Tooltip("the acceleration of the camera yaw while in freelook")]
    public float FreeLook_YawAcceleration;

    [Tooltip("the speed the free look camera pitches")]
    public float FreeLook_PitchSpeed;

    [Tooltip("the acceleration of the camera pitch while in freelook")]
    public float FreeLook_PitchAcceleration;

    // TODO: very weird for this to be smaller than min pitch
    [Tooltip("the minimum pitch when in free look mode")]
    public float FreeLook_MinPitch;

    [Tooltip("the maximum pitch when in free look mode")]
    public float FreeLook_MaxPitch;

    [Tooltip("the distance change when undershooting the min pitch angle (gets closer to the character)")]
    public AnimationCurve Distance_PitchCurve;

    [Tooltip("the delay in seconds after free look when the camera returns to active mode")]
    public float FreeLook_Timeout;

    [Tooltip("the delay (in s) during a move intent until the camera resumes tracking")]
    public float FreeLook_MoveIntentTimeout;

    [Tooltip("the delay in seconds after free look when the camera returns to active mode")]
    public float FreeLook_OvershootLookUp;

    [Tooltip("the minimum distance from the target, when undershooting")]
    public float MinUndershootDistance;

    // TODO: this is the camera's radius
    // TODO: make all the camera's casts sphere casts
    [Header("collision")]
    [Tooltip("the amount of offset the camera during collision")]
    public float Collision_ContactOffset;

    [Tooltip("the collision mask for the camera with the world")]
    public LayerMask Collision_Mask;

    [Tooltip("how far away the corrected collision position should be from the ideal position before clipping")]
    public float Collision_ClipTolerance;

    [Tooltip("how far away the corrected collision position should be from the ideal position before clipping, scaled by hit normal")]
    public MapOutCurve Collision_ClipToleranceByNormal;

    [Tooltip("the exit duration before clipping transitions back to free look")]
    public float Collision_ClipCooldown;

    [Tooltip("the velocity damping the moment the camera clips")]
    public AdsrCurve Collision_ClipDamping;

    [Tooltip("the rate of change of local distance of the camera to the target, if correcting")]
    public float Collision_FreeLook_CorrectionSpeed;

    [Tooltip("the rate of change of local distance of the camera to the target, if correcting, when tracking")]
    public float Collision_Tracking_CorrectionSpeed;

    // -- recenter --
    [Header("recenter")]
    [Tooltip("the camera's yaw acceleration when recentering")]
    public float Recenter_YawAcceleration;

    [Tooltip("the speed the camera recenters after idle")]
    public float Recenter_YawSpeed;

    [Tooltip("the curve the camera recenters looking at the character")]
    public AnimationCurve Recenter_YawCurve;

    // -- tilt/dutch --
    [Header("tilt/dutch")]
    [Tooltip("the camera dutch angle (around z-axis) scale applied to the camera's target's rotation")]
    public float DutchScale;

    [Tooltip("the smoothing on the camera dutch angle (around z-axis)")]
    public float DutchSmoothing;

    // -- lifecycle --
    void OnValidate() {
        Tracking_MaxPitch = Mathf.Max(Tracking_MinPitch, Tracking_MaxPitch);
        FreeLook_MinPitch = Mathf.Min(FreeLook_MinPitch, Tracking_MinPitch);
        FreeLook_MaxPitch = Mathf.Max(FreeLook_MinPitch, FreeLook_MaxPitch);
    }
}

}