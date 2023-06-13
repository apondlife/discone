using UnityEngine;

namespace Discone {
public class IcecreamFace : MonoBehaviour
{
    [Header("config")]
    [SerializeField] float m_BlinkSpeed;

    [Header("refs")]
    [SerializeField] Renderer m_Renderer;

    // TODO: make fun blink
    void Update()
    {
        m_Renderer.material.SetInteger(ShaderProps.CurrentSprite, Mathf.FloorToInt(Time.time * m_BlinkSpeed));
    }
}
}