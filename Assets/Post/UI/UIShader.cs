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
    [SerializeField] float dissolveAmount;

    // -- lifecycle --
    void Awake() {
        m_Material = GetComponent<RawImage>().material;
    }


    void Update() {
        m_Material.SetFloat("_DissolveAmount", dissolveAmount);
    }
}
