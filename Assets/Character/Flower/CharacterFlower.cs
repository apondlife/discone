using UnityEngine;
using Mirror;
using System.Collections.Generic;
using ThirdPerson;

/// a flower that a character leaves behind as its checkpoint
[RequireComponent(typeof(Renderer))]
public class CharacterFlower: NetworkBehaviour {
    // -- constants --
    /// how offset the flower is forward, so it doesn't spawn under the character
    const float k_ForwardOffset = 0.12f;

    /// how offset the flower is up, so it can find a ground
    const float k_UpOffset = 0.05f;

    /// how much to raycast down to find the ground
    const float k_RaycastLen = 5f;

    /// pre-allocated buffer for ground raycasts
    static RaycastHit[] k_Hits = new RaycastHit[1];

    /// pre-allocated buffer for ground raycasts
    static LayerMask k_GroundMask =>
        LayerMask.NameToLayer("Default")
        | LayerMask.NameToLayer("Ground")
        | LayerMask.NameToLayer("Indoor");

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
    bool m_IsFree = true;

    // -- refs --
    [Header("refs")]
    [Tooltip("the renderer for the flower")]
    [SerializeField] Renderer m_Renderer;

    // -- props --
    /// the assosciated character's key
    CharacterKey m_Key;

    /// the checkpoint position
    Vector3 m_Position;

    /// the checkpoint rotation
    Quaternion m_Rotation;

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
        m_Renderer.material = FindMaterial();
    }

    // -- queries --
    /// the associated character's key
    public CharacterKey Key {
        get => m_Key;
    }

    /// the flower's position
    public Vector3 Position {
        get => m_Position;
    }

    /// the flower's rotation
    public Quaternion Rotation  {
        get => m_Rotation;
    }

    /// if this flower is free
    public bool IsFree {
        get => m_IsFree;
    }

    /// find cached material for texture and saturation
    Material FindMaterial() {
        return FindMaterial(m_IsFree ? m_Saturation : 1.0f);
    }

    Material FindMaterial(float saturation) {
        var key = $"{m_Texture.name}/{saturation}";

        // create instanced material for the texture if not cached
        if (!s_MaterialCache.TryGetValue(key, out var material)) {
            material = Instantiate(m_Renderer.sharedMaterial);
            material.mainTexture = m_Texture;
            material.SetFloat("_Saturation", saturation);
            material.name = key;

            s_MaterialCache.Add(key, material);
        }

        return material;
    }

    // -- factories --
    /// spawn a flower from a record
    [Server]
    public static CharacterFlower Server_Spawn(FlowerRec rec) {
        return Server_Spawn(rec.Key, rec.Pos, rec.Rot);
    }

    /// spawn a flower from key and transform
    [Server]
    public static CharacterFlower Server_Spawn(
        CharacterKey key,
        Vector3 pos,
        Quaternion rot
    ) {
        var prefab = CharacterDefs.Instance.Find(key)?.Flower;
        if (prefab == null) {
            Debug.LogError($"[flower] no prefab found for {key.Name()}");
            return null;
        }

        // find ground position
        var fwd = rot * Vector3.forward;
        var hits = Physics.RaycastNonAlloc(
            pos + fwd * k_ForwardOffset + Vector3.up * k_UpOffset,
            Vector3.down,
            k_Hits,
            k_RaycastLen,
            k_GroundMask,
            QueryTriggerInteraction.Ignore
        );

        if (hits <= 0) {
            Debug.LogError($"[flower] failed to find ground point");
        }

        // instantiate flower at hit point
        var hit = hits > 0 ? k_Hits[0].point : pos;
        var flower = Instantiate(
            prefab,
            hit,
            Quaternion.identity
        );

        flower.m_Key = key;
        flower.m_Position = pos;
        flower.m_Rotation = rot;
        flower.m_IsFree = true;

        #if UNITY_EDITOR
        flower.name = $"Flower_{key.Name()}";
        #endif

        // spawn the game object for everyone
        NetworkServer.Spawn(flower.gameObject);

        return flower;
    }

    // -- factories --
    /// create state frame from this flower
    public CharacterState.Frame IntoState() {
        return new CharacterState.Frame(
            m_Position,
            m_Rotation * Vector3.forward
        );
    }

    /// construct a record from this flower
    public FlowerRec IntoRecord() {
        return new FlowerRec(
            Key,
            m_Position,
            m_Rotation
        );
    }
}