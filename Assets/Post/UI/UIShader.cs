using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityAtoms.BaseAtoms;

/// blit a post processing effect
[ExecuteInEditMode]
[RequireComponent(typeof(RawImage))]
public class UIShader: MonoBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("the post-processing material (shader)")]
    [SerializeField] Material m_Material;

    // [Range(0, 1)]
    // [SerializeField] public float dissolveAmount;

    // [Range(0, 1f)]
    // [SerializeField] public float letterboxAmount;

    [SerializeField] FloatVariable m_DissolveAmount;

    [SerializeField] FloatVariable m_LetterboxAmount;


    // -- lifecycle --
    void Awake() {
        m_Material = GetComponent<RawImage>().material;
    }


    void Update() {
        m_Material.SetFloat("_DissolveAmount", m_DissolveAmount.Value);
        m_Material.SetFloat("_LetterboxAmount", m_LetterboxAmount.Value);
    }
}
