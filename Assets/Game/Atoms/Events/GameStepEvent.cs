using UnityEngine;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// Event of type `GameStep`. Inherits from `AtomEvent&lt;GameStep&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-cherry")]
    [CreateAssetMenu(menuName = "Unity Atoms/Events/GameStep", fileName = "GameStepEvent")]
    public sealed class GameStepEvent : AtomEvent<GameStep>
    {
    }
}
