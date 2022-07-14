using UnityEngine;
using Mirror;
using System.Collections.Generic;

/// a flower that a character leaves behind as its checkpoint
[RequireComponent(typeof(Renderer))]
class CharacterFlower: NetworkBehaviour {
    // -- statics --
    /// the cache of per-texture materials
    static Dictionary<string, Material> s_MaterialCache = new Dictionary<string, Material>();

    // -- config --
    [Header("config")]
    [Tooltip("the texture to use for the flower")]
    [SerializeField] private Texture m_Texture;

    [Tooltip("the saturation of the released flower")]
    [SerializeField] private float m_Saturation = 0.8f;

    [SyncVar(hook = nameof(Client_OnIsFreeReceieved))]
    private bool m_IsFree = false;

    // -- refs --
    [Header("refs")]
    [Tooltip("the renderer for the flower")]
    [SerializeField] private Renderer m_Renderer;

    // -- lifecycle
    private void Awake() {
        // create instanced material for the texture if not cached
        if (!s_MaterialCache.TryGetValue(m_Texture.name, out var material)) {
            material = Instantiate(m_Renderer.sharedMaterial);
            material.mainTexture = m_Texture;
            s_MaterialCache.Add(m_Texture.name, material);
        }

        m_Renderer.material = material;
    }


    // -- commands --
    // called when the flower is no longer owned by a character
    public void Server_Release() {
        m_IsFree = true;
    }

    // -- events --
    private void Client_OnIsFreeReceieved(bool oldFree, bool newFree) {
        if (newFree) {
            m_Renderer.material.SetFloat("_Saturation", m_Saturation);
        }
    }
}