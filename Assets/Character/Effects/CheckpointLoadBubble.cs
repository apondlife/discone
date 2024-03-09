using Soil;
using UnityEngine;

namespace Discone {

/// the checkpoint loading bubble effect
[RequireComponent(typeof(Renderer))]
public class CheckpointLoadBubble: MonoBehaviour {
    // -- types --
    enum State {
        Disabled,
        EaseIn,
        Hold,
        EaseOut
    }

    // -- tuning --
    [Header("tuning")]
    [Tooltip("bubble growth ease in curve")]
    [SerializeField] EaseTimer m_EaseIn;

    [Tooltip("bubble growth ease out curve")]
    [SerializeField] EaseTimer m_EaseOut;

    [Tooltip("the emission texture animation speed")]
    [SerializeField] Vector2 m_EmissionOffsetSpeed;

    // -- cfg --
    [Header("cfg")]
    [Tooltip("the anchor transform")]
    [SerializeField] Transform m_Anchor;

    // -- props --
    /// the character
    DisconeCharacter m_Character;

    /// the bubble renderer
    Renderer m_Renderer;

    /// the bubble renderer
    Material m_Material;

    /// the scale the ease timer eases around (transform.localscale)
    Vector3 m_BaseScale;

    /// the state of the animation
    State m_State = State.Disabled;

    // -- lifecycle --
    void Awake() {
        // set props
        m_Character = GetComponentInParent<DisconeCharacter>();
        m_Renderer = GetComponent<Renderer>();
        m_Material = m_Renderer.material;
        m_BaseScale = transform.localScale;

        // re-parent to the bone anchor
        if (m_Anchor != null) {
            transform.SetParent(m_Anchor, true);
        }

        // set initial state
        m_Renderer.enabled = false;
    }

    void Update() {
        // activate ease if loading
        switch (m_State) {
            case State.Disabled:
                if (m_Character.Checkpoint.IsLoading) {
                    m_EaseIn.Start();
                    m_Renderer.enabled = true;
                    m_State = State.EaseIn;
                }
                break;
            case State.EaseIn:
                m_EaseIn.Tick();
                transform.localScale = m_BaseScale * m_EaseIn.Pct;
                if (!m_EaseIn.IsActive) {
                    m_State = State.Hold;
                }
                break;
            case State.Hold:
                if (!m_Character.Checkpoint.IsLoading) {
                    m_EaseOut.Start();
                    m_State = State.EaseOut;
                }
                break;
            case State.EaseOut:
                m_EaseOut.Tick();
                transform.localScale = m_BaseScale * m_EaseOut.Pct;
                if (!m_EaseOut.IsActive) {
                    m_Renderer.enabled = false;
                    m_State = State.Disabled;
                }
                break;
        }

        if (m_State != State.Disabled) {
            m_Material.SetTextureOffset(
                ShaderProps.Main,
                Time.time * m_EmissionOffsetSpeed
            );
        }
    }
}

}