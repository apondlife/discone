using UnityEngine;

namespace ThirdPerson {

[RequireComponent(typeof(Projector))]
public class CharacterBlobShadow : MonoBehaviour {

    // -- fields --
    [SerializeField]
    [UnityEngine.Serialization.FormerlySerializedAs("ResizeCurve")]
    private AnimationCurve m_ResizeCurve;

    [SerializeField]
    private float m_MinSize;

    [SerializeField]
    private CameraLookAtTarget m_GroundTarget;

    // -- props --
    private Projector m_Projector;
    private float m_BaseSize;
    private CharacterState m_State;

    // -- lifetime --
    private void Start() {
        m_Projector = GetComponent<Projector>();
        m_BaseSize = m_Projector.orthographicSize;
        m_State = GetComponentInParent<Character>().State;
    }

    private void Update() {

        m_Projector.orthographicSize = Mathf.Lerp(m_MinSize, m_BaseSize, m_ResizeCurve.Evaluate(m_GroundTarget.PercentExtended));
        m_Projector.enabled = !m_State.IsGrounded;
    }
}
}