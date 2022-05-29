using UnityEngine;
using UnityEngine.UI;
using UnityAtoms.BaseAtoms;

/// blit a post processing effect
[ExecuteAlways]
[RequireComponent(typeof(RawImage))]
class UIShader: MonoBehaviour {
    // -- state --
    [Header("state")]
    [Tooltip("the percent to dissolve the text")]
    [SerializeField] FloatReference m_DissolveAmount;

    [Tooltip("the percent to letterbox the text")]
    [SerializeField] FloatReference m_LetterboxAmount;

    // -- refs --
    [Header("refs")]
    [Tooltip("the post-processing material (shader)")]
    [SerializeField] RawImage m_Image;

    // -- props --
    /// the underlying material
    Material m_Material;

    // -- lifecycle --
    void Awake() {
        m_Material = m_Image.material.Unsaved();
        m_Image.material = m_Material;
    }

    void Update() {
        m_Material.SetFloat("_DissolveAmount", m_DissolveAmount.Value);
        m_Material.SetFloat("_LetterboxAmount", m_LetterboxAmount.Value);
    }
}
