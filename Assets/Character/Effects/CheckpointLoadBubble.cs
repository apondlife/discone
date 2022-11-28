using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Discone {

[RequireComponent(typeof(Renderer))]
public class CheckpointLoadBubble : MonoBehaviour
{
    enum State {
        disabled,
        easeIn,
        hold,
        easeOut
    }

    [Tooltip("bubble growth ease in curve")]
    [SerializeField] ThirdPerson.EaseTimer m_EaseIn;

    [Tooltip("bubble growth ease out curve")]
    [SerializeField] ThirdPerson.EaseTimer m_EaseOut;

    /// the bubble renderer
    Renderer m_Bubble;

    /// the character
    DisconeCharacter m_Character;

    /// the scale the ease timer eases around (transform.localscale)
    Vector3 m_BaseScale;

    /// the state of the animation
    State m_State = State.disabled;

    // -- lifecycle --
    void Awake()
    {
        m_Bubble = GetComponent<Renderer>();
        m_Character = GetComponentInParent<DisconeCharacter>();
        m_Bubble.enabled = false;
        m_BaseScale = transform.localScale;
    }

    void Update()
    {
        // activate ease if loading
        switch (m_State) {
            case State.disabled:
                if (m_Character.Checkpoint.IsLoading) {
                    m_EaseIn.Start();
                    m_Bubble.enabled = true;
                    m_State = State.easeIn;
                }
                break;
            case State.easeIn:
                m_EaseIn.Tick();
                transform.localScale = m_BaseScale * m_EaseIn.Pct;
                if (!m_EaseIn.IsActive) {
                    m_State = State.hold;
                }
                break;
            case State.hold:
                if (!m_Character.Checkpoint.IsLoading) {
                    m_EaseOut.Start();
                    m_State = State.easeOut;
                }
                break;
            case State.easeOut:
                m_EaseOut.Tick();
                transform.localScale = m_BaseScale * m_EaseOut.Pct;
                if (!m_EaseOut.IsActive) {
                    m_Bubble.enabled = false;
                    m_State = State.disabled;
                }
                break;
        }
    }
}

}