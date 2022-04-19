using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

/// blit a post processing effect
[ExecuteInEditMode]
[RequireComponent(typeof(RawImage))]
public class UIShader: MonoBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("the post-processing material (shader)")]
    [SerializeField] Material m_Material;

    [Range(0, 1)]
    [SerializeField] public float dissolveAmount;

    [Range(0, 1f)]
    [SerializeField] public float letterboxAmount;

    // -- lifecycle --
    void Awake() {
        m_Material = GetComponent<RawImage>().material;
    }


    void Update() {
        m_Material.SetFloat("_DissolveAmount", dissolveAmount);
        m_Material.SetFloat("_LetterboxAmount", letterboxAmount);
    }
}
