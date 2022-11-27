using UnityEngine;
using UnityEngine.Serialization;

namespace ThirdPerson {

[RequireComponent(typeof(Projector))]
public class CharacterBlobShadow : MonoBehaviour {

    // -- fields --
    [Header("cfg")]
    [Tooltip("the layer mask for the ground")]
    [SerializeField] LayerMask m_GroundLayers;

    [Tooltip("a transform for the character's feet")]
    [SerializeField] Transform m_CharacterFeet;

    [Header("resize")]
    [Tooltip("the curve for how the blob should resize based on distance")]
    [SerializeField] AnimationCurve m_ResizeCurve;

    [Tooltip("the maximum distance for the resize curve")]
    [FormerlySerializedAs("m_MaxDistance")]
    [SerializeField] float m_MaxResizeDistance;

    [Header("fade")]
    [Tooltip("the curve for how the blob should fade based on distance")]
    [SerializeField] AnimationCurve m_FadeCurve;

    [Tooltip("the maximum alpha value for the shadow fadind")]
    [SerializeField] float m_MaxAlpha;

    [Tooltip("the maximum distance for the fade curve")]
    [SerializeField] float m_MaxFadeDistance;

    // -- props --
    /// the projector
    Projector m_Projector;

    /// the size of the projector
    float m_BaseSize;

    /// the character's state
    CharacterState m_State;

    /// the distance the feet is from this transform
    float m_FeetDistance;

    /// the color of the projector
    Color m_ProjectorColor;

    /// the material of the projector
    Material m_ProjectorMaterial;

    private void OnValidate() {
        if(m_CharacterFeet == null) {
            return;
        }

        var position = transform.position;
        if (transform.position.x != m_CharacterFeet.position.x) {
            position.x = m_CharacterFeet.position.x;
        }

        if (transform.position.z != m_CharacterFeet.position.z) {
            position.z = m_CharacterFeet.position.z;
        }

        transform.position = position;
    }


    // -- lifetime --
    private void Start() {
        m_Projector = GetComponent<Projector>();
        m_BaseSize = m_Projector.orthographicSize;
        m_State = GetComponentInParent<Character>().State;
        m_FeetDistance = Mathf.Abs(transform.position.y - m_CharacterFeet.position.y);

        m_ProjectorMaterial = Instantiate(m_Projector.material);
        m_Projector.material = m_ProjectorMaterial;

        m_ProjectorColor = m_Projector.material.color;
    }

    private void Update() {
        var dist = m_MaxResizeDistance;
        var didHit = Physics.Raycast(transform.position, Vector3.down, out var hit, m_MaxResizeDistance + m_FeetDistance, m_GroundLayers);
        if (didHit) {
            dist = Mathf.Max(0, Vector3.Distance(transform.position, hit.point) - m_FeetDistance);
        }

        // set size
        m_Projector.orthographicSize = Mathf.Lerp(0, m_BaseSize, m_ResizeCurve.Evaluate(dist / m_MaxResizeDistance));

        // set fade
        m_ProjectorColor.a = Mathf.Lerp(0, m_MaxAlpha, m_FadeCurve.Evaluate(dist / m_MaxFadeDistance));
        m_Projector.material.color = m_ProjectorColor;

        m_Projector.enabled = !m_State.IsGrounded;
    }
}
}