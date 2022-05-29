using UnityEngine;
using ThirdPerson;

// Dumb AI that runs in a circle
[System.Serializable]
public class AiInputSource: CharacterInputSource {
    // -- fields --
    [Tooltip("the move direction")]
    [SerializeField] private Vector2 m_Direction = new Vector2(0.7f, 0.7f);

    [Tooltip("the probability of jumping each frame")]
    [SerializeField] private float m_JumpProbability = 0.001f;

    // -- CharacterInputSource --
    public bool IsEnabled {
        get => true;
    }

    public CharacterInput.Frame Read() {
        return new CharacterInput.Frame(
            Random.value < m_JumpProbability,
            m_Direction
        );
    }
}