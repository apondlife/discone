using UnityEngine;

namespace Discone {
public class IcecreamFace : MonoBehaviour
{
    // -- constants --
    /// how many sprites are in the blink animation
    const int k_NumBlinkSprites = 4;

    /// index for the neutral face (i.e. open eyes)
    const int k_NeutralFaceSpriteIndex = 0;

    [Header("config")]
    [Tooltip("the time it takes for a blink")]
    [SerializeField] float m_BlinkTime;

    [Tooltip("minimum time between blinks (in seconds)")]
    [SerializeField] float m_MinTimeBetweenBlinks;
    
    [Tooltip("maximum time between blinks (in seconds)")]
    [SerializeField] float m_MaxTimeBetweenBlinks;
    
    [Header("refs")]
    [SerializeField] Renderer m_Renderer;

    // index for the current sprite in the blink animation
    int m_CurrentSpriteIndex = 0;

    /// the blink animation
    Coroutine m_Blink;

    void Start()
    {
        m_CurrentSpriteIndex = k_NeutralFaceSpriteIndex;

        BlinkAfterRandomInterval();
    }
    // TODO: make fun blink
    void Update()
    {
        m_Renderer.material.SetInteger(ShaderProps.CurrentSprite, m_CurrentSpriteIndex);
    }

    void BlinkAfterRandomInterval()  {
        float interval = Random.Range(m_MinTimeBetweenBlinks, m_MaxTimeBetweenBlinks);
        this.DoAfterTime(interval, DoBlink);
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
                BlinkAfterRandomInterval();
            }
        ));

    }
}
}