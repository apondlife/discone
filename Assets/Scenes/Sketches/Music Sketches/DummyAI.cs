using UnityEngine;
using UnityEngine.InputSystem;
using ThirdPerson;

// Dumb AI that runs in a circle

[System.Serializable]
public class DummyAI : ThirdPerson.InputProvider {
    // -- fields --
    [Tooltip("probability of jumping per frame")]
    [SerializeField] private float jumpProbability = 0.001f;

    public override Vector2 Move {
        get => Vector2.right; // always move right
    }

    public override bool IsJumpPressed {
        get => Random.value < jumpProbability;
    }
}