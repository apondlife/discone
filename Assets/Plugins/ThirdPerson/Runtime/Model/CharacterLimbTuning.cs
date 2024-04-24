using Soil;
using UnityEngine;

namespace ThirdPerson {

[CreateAssetMenu(fileName = "CharacterLimbTuning", menuName = "thirdperson/CharacterLimbTuning")]
public class CharacterLimbTuning: ScriptableObject {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the layer mask")]
    public LayerMask CastMask;

    [Tooltip("the cast offset")]
    public float CastOffset;

    // -- tuning --
    [Header("tuning - blend")]
    [Tooltip("the extra velocity when blending ik as a function of input")]
    public float Blend_InputVelocity;

    [Tooltip("the speed the ik weight blends towards one")]
    public float Blend_InSpeed;

    [Tooltip("the speed the ik weight blends towards zero")]
    public float Blend_OutSpeed;

    // -- tuning --
    [Header("tuning - stride")]
    [Tooltip("the threshold under which movements are ignored")]
    public float MinMove;

    [Tooltip("the max length (radius) of the stride")]
    public FloatRange MaxLength;

    [Tooltip("the max length (radius) of the stride on the cross axis")]
    public float MaxLength_CrossScale;

    [Tooltip("the shape of the stride as a fn of progress through the complete stride")]
    public AnimationCurve Shape;

    [Tooltip("the release speed on the stride scale as a fn of input")]
    public float InputScale_ReleaseSpeed;

    [Tooltip("the stride scale as a fn of speed")]
    public MapCurve SpeedScale;

    [Tooltip("the extra search distance when on a surface")]
    public float SearchRange_Surface;

    [Tooltip("the extra search distance when not on a surface")]
    public float SearchRange_NoSurface;
}


}