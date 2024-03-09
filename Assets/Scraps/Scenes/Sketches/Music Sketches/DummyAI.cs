using System;
using ThirdPerson;
using UnityEngine;

// dumb ai that runs in a circle
namespace Discone {

[Serializable]
public class AiInputSource: PlayerInputSource<InputFrame> {
    // -- fields --
    [Tooltip("the move direction")]
    [SerializeField] Vector2 m_Move = new Vector2(0.7f, 0.7f);

    [Tooltip("the probability of jumping each frame")]
    [SerializeField] float m_JumpProbability = 0.001f;

    // -- CharacterInputSource --
    public override bool IsEnabled {
        get => true;
    }

    public override InputFrame Read() {
        return new InputFrame(
            new CharacterInputMain(
                m_Move,
                UnityEngine.Random.value < m_JumpProbability,
                false
            )
        );
    }
}

}