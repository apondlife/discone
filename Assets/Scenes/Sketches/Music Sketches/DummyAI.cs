using UnityEngine;
using UnityEngine.InputSystem;
using ThirdPerson;

// Dumb AI that runs in a circle

[System.Serializable]
public class DummyAI : ThirdPerson.InputProvider {
    // -- fields --
    [Tooltip("input direction")]
    [SerializeField] private Vector2 inputDirection = new Vector2(0.7f, 0.7f);

    [Tooltip("probability of jumping per frame")]
    [SerializeField] private float jumpProbability = 0.001f;

    public override Vector2 Move {
        get => inputDirection;
    }

    public override bool IsJumpPressed {
        get => Random.value < jumpProbability;
    }
}