using UnityEngine;

namespace Discone {

[CreateAssetMenu(fileName = "CheckpointTuning", menuName = "discone/CheckpointTuning")]
public class CheckpointTuning: ScriptableObject {
    [Tooltip("how far from a checkpoint can you grab it")]
    public float GrabRadius;

    // -- save --
    [Header("save")]
    [Tooltip("the time (s) to start smell after crouch")]
    public float Save_Delay;

    [Tooltip("the time (s) to smell a flower")]
    public float Save_SmellTime;

    [Tooltip("the time (s) to plant a flower")]
    public float Save_PlantTime;

    // -- save/queries
    /// the time (s) to smell a flower after delay
    public float Save_SmellDuration {
        get => Save_SmellTime - Save_Delay;
    }

    /// the time (s) to plant a flower after smell
    public float Save_PlantDuration {
        get => Save_PlantTime - Save_SmellDuration;
    }

    // -- load --
    [Header("load")]
    [Tooltip("the max load duration")]
    public float Load_CastMaxTime;

    [Tooltip("the load duration at the point distance")]
    public float Load_CastPointTime;

    [Tooltip("the distance at the point duration")]
    public float Load_CastPointDistance;

    [Tooltip("the time multiplier when unloading")]
    public float Load_CancelMultiplier;
}

}