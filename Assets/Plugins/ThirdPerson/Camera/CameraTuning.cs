using UnityEngine;

namespace ThirdPerson {

[CreateAssetMenu(fileName = "CameraTuning", menuName = "thirdperson/CameraTuning", order = 1)]
sealed class CameraTuning: ScriptableObject {
    // -- axis --
    [Header("axis")]
    [Tooltip("if the x-axis is inverted")]
    public bool IsInvertedX;

    [Tooltip("if the y-axis is inverted")]
    public bool IsInvertedY;

    // -- movement --
    [Header("movement")]
    [Tooltip("the collision mask for the camera with the world")]
    public LayerMask CollisionMask;

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

    [Tooltip("the rate of change of local distance of the camera to the target, if correcting")]
    public float CorrectionSpeed;

    [Tooltip("the smooth time for moving the camera to target, if correcting")]
    public float CorrectionSmoothTime = 0.5f;

    // TODO: this is the camera's radius
    // TODO: make all the camera's casts sphere casts
    [Tooltip("the amount of offset the camera during collision")]
    public float ContactOffset;

    // -- target speed --
    [Header("target speed")]
    [Tooltip("the camera distance multiplier as a function of target speed")]
    public AnimationCurve DollySpeedCurve;

    [Tooltip("the minimum speed for curving camera distance")]
    public float DollyTargetMinSpeed;

    [Tooltip("the maximum speed for curving camera distance")]
    public float DollyTargetMaxSpeed;

    [Tooltip("the maximum speed to camera distance")]
    public float MaxRadius;

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

    [Tooltip("the speed the camera distance adjusts in freelook")]
    public float DollySpeed;

    // TODO: very weird for this to be smaller than min pitch
    [Tooltip("the minimum pitch when in free look mode")]
    public float FreeLook_MinPitch;

    [Tooltip("the maximum pitch when in free look mode")]
    public float FreeLook_MaxPitch;

    [Tooltip("the distance change when undershooting the min pitch angle (gets closer to the character)")]
    public AnimationCurve Distance_PitchCurve;

    [Tooltip("the minimum distance from the target, when undershooting")]
    public float MinUndershootDistance;

    [Tooltip("the delay in seconds after free look when the camera returns to active mode")]
    public float FreeLook_Timeout;

    [Tooltip("the delay (in s) during a move intent until the camera resumes tracking")]
    public float FreeLook_MoveIntentTimeout;

    [Tooltip("the delay in seconds after free look when the camera returns to active mode")]
    public float FreeLook_OvershootLookUp;

    // -- recenter --
    [Header("recenter")]
    [Tooltip("the time the camera can be idle before recentering")]
    public float Recenter_IdleTime;

    [Tooltip("the speed the camera recenters after idle")]
    public float Recenter_YawSpeed;

    [Tooltip("the curve the camera recenters looking at the character")]
    public AnimationCurve Recenter_YawCurve;

    void OnValidate() {
        Tracking_MaxPitch = Mathf.Max(Tracking_MinPitch, Tracking_MaxPitch);
        FreeLook_MinPitch = Mathf.Min(FreeLook_MinPitch, Tracking_MinPitch);
        FreeLook_MaxPitch = Mathf.Max(FreeLook_MinPitch, FreeLook_MaxPitch);
    }
}
}