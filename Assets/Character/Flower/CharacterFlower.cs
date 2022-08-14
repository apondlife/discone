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
    [SerializeField] Texture m_Texture;

    [Tooltip("the saturation of the released flower")]
    [SerializeField] float m_Saturation = 0.8f;

    [Tooltip("the saturation of the released flower")]
    [SerializeField] float m_SpawnTime = 0.5f;

    [SyncVar(hook = nameof(Client_OnIsFreeReceieved))]
    bool m_IsFree = false;

    // -- refs --
    [Header("refs")]
    [Tooltip("the renderer for the flower")]
    [SerializeField] Renderer m_Renderer;

    // -- lifecycle
    void Awake() {
        m_Renderer.material = FindMaterial();

        #if UNITY_EDITOR
        Dbg.AddToParent("Flowers", this);
        #endif

        var targetScale = transform.localScale;
        transform.localScale = Vector3.Scale(transform.localScale, new Vector3(1, 0, 1));
        StartCoroutine(CoroutineHelpers.InterpolateByTime(m_SpawnTime, (k) => {
            transform.localScale = Vector3.Scale(targetScale, new Vector3(1, k*k, 1));
        }));
    }



    // -- commands --
    // called when the flower is no longer owned by a character
    public void Server_Release() {
        m_IsFree = true;
    }

    // -- events --
    void Client_OnIsFreeReceieved(bool oldFree, bool newFree) {
        if (newFree) {
            m_Renderer.material = FindMaterial(m_Saturation);
        }
    }

    // -- queries --
    /// find cached material for texture and saturation
    Material FindMaterial(float saturation = 1.0f) {
        var key = $"{m_Texture.name}/{saturation}";

        // create instanced material for the texture if not cached
        if (!s_MaterialCache.TryGetValue(key, out var material)) {
            material = Instantiate(m_Renderer.sharedMaterial);
            material.mainTexture = m_Texture;
            material.SetFloat("_Saturation", saturation);

            s_MaterialCache.Add(key, material);
        }

        return material;
    }
}