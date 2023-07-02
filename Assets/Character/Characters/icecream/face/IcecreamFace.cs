using UnityEngine;

namespace Discone {
public class IcecreamFace : MonoBehaviour
{
    // -- constants --
    /// how many sprites are in the blink animation
    const int k_NumBlinkSprites = 4;

    /// index for the neutral face (i.e. open eyes)
    // TODO: this should be 0... is there a bug in the shader code
    const int k_NeutralFaceSpriteIndex = 2;

    [Header("config")]
    [SerializeField] float m_BlinkSpeed;

    [Header("refs")]
    [SerializeField] Renderer m_Renderer;

    // index for the current sprite in the blink animation
    int m_CurrentSpriteIndex = 0;

    /// the blink animation
    Coroutine m_Blink;

    // -- blink --
    [Header("cfg/blink")]
    [Tooltip("the time it takes for a blink")]
    [SerializeField] float m_BlinkTime = 0.5f;

    void Start()
    {
        m_CurrentSpriteIndex = k_NeutralFaceSpriteIndex;

        // TODO: uncomment to turn on blinking
        // this.DoAfterTime(1f, DoBlink);
    }
    // TODO: make fun blink
    void Update()
    {
        // Debug.Log(" sprite = " + m_CurrentSpriteIndex);
        m_Renderer.material.SetInteger(ShaderProps.CurrentSprite, m_CurrentSpriteIndex);
    }

    
    void BlinkAfterRandomInterval()  {
        float interval = Random.Range(2, 4);
        this.DoAfterTime(1f, DoBlink);
    }
    
    void DoBlink() {
        m_Blink = StartCoroutine(CoroutineHelpers.InterpolateByTime(
            m_BlinkTime,

            (k) => {
                float mapped = Mathf.Lerp(0, k_NumBlinkSprites, k);
                m_CurrentSpriteIndex = Mathf.FloorToInt(mapped);
            },

            () => {
                m_CurrentSpriteIndex = k_NeutralFaceSpriteIndex;
                m_Blink = null;
                this.DoAfterTime(1f, DoBlink);
            }
        ));

    }
}
}