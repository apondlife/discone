using UnityEngine;
using Mirror;
using System.Collections.Generic;

/// a flower that a character leaves behind as its checkpoint
[RequireComponent(typeof(Renderer))]
public class CharacterFlower: NetworkBehaviour {
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

    // -- props --
    /// the assosciated character's key
    CharacterKey m_Key;

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
    /// release the server from player ownership
    [Server]
    public void Server_Release() {
        m_IsFree = true;
    }

    [Server]
    public void Server_Grab() {
        m_IsFree = false;
    }

    // -- events --
    /// when the free state changes
    [Client]
    void Client_OnIsFreeReceieved(bool oldFree, bool newFree) {
        if (newFree) {
            m_Renderer.material = FindMaterial(m_Saturation);
        }
    }

    // -- queries --
    /// the associated character's key
    public CharacterKey Key {
        get => m_Key;
    }

    /// if this flower is free
    public bool IsFree {
        get => m_IsFree;
    }

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

    // -- factories --
    /// spawn a flower from a record
    [Server]
    public static void Spawn(FlowerRec rec) {
        Spawn(rec.Key, rec.Pos);
    }

    /// spawn a flower from key and pos
    [Server]
    public static void Spawn(CharacterKey key, Vector3 pos) {
        var prefab = CharacterDefs.Instance.Find(key)?.Flower;
        if (prefab == null) {
            Debug.LogError($"[World] no flower prefab found for {key.Name()}");
            return;
        }

        var instance = Instantiate(
            prefab,
            pos,
            Quaternion.identity
        );

        instance.m_Key = key;

        #if UNITY_EDITOR
        instance.name = $"Flower_{key.Name()}";
        #endif

        // spawn the game object for everyone
        NetworkServer.Spawn(instance.gameObject);
    }
}