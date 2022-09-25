using UnityEngine;
using Mirror;
using System.Collections.Generic;
using ThirdPerson;
using UnityAtoms;

/// a flower that a character leaves behind as its checkpoint
[RequireComponent(typeof(Renderer))]
public class CharacterFlower: NetworkBehaviour {
    // -- types --
    /// a flower's planting state
    enum Planting {
        NotReady,
        Ready,
        Planted
    }

    // -- constants --
    /// how offset the flower is forward, so it doesn't spawn under the character
    const float k_ForwardOffset = 0.12f;

    /// how offset the flower is up, so it can find a ground
    const float k_UpOffset = 0.05f;

    /// how much to raycast down to find the ground
    const float k_RaycastLen = 5.0f;

    /// the cache of per-texture materials
    static readonly Dictionary<string, Material> k_MaterialCache = new Dictionary<string, Material>();

    /// the ground layer mask
    static LayerMask s_GroundMask;

    // -- cfg --
    [Header("cfg")]
    [Tooltip("the texture to use for the flower")]
    [SerializeField] Texture m_Texture;

    [Tooltip("the saturation of the released flower")]
    [SerializeField] float m_Saturation = 0.8f;

    [Tooltip("the saturation of the released flower")]
    [SerializeField] float m_SpawnTime = 0.5f;

    // -- fields --
    [Header("fields")]
    [Tooltip("the checkpoint this flower represents")]
    [SyncVar(hook = nameof(Client_OnCheckpointReceived))]
    [ReadOnly] [SerializeField] Checkpoint m_Checkpoint;

    // -- published --
    [Header("published")]
    [Tooltip("the event called when a flower gets planted")]
    [SerializeField] CharacterFlowerEvent m_FlowerPlanted;

    // -- refs --
    [Header("refs")]
    [Tooltip("the renderer for the flower")]
    [SerializeField] Renderer m_Renderer;

    // -- props --
    /// the assosciated character's key
    [SyncVar]
    CharacterKey m_Key;

    /// if the flower has been planted
    Planting m_Planting = Planting.NotReady;

    /// if no player is using this flower
    [SyncVar(hook = nameof(Client_OnIsFreeReceieved))]
    bool m_IsFree = true;

    // -- lifecycle
    void Awake() {
        m_Renderer.material = FindMaterial();

        // every byte counts
        if (s_GroundMask == 0) {
            s_GroundMask = LayerMask.GetMask("Default", "Field", "Indoor");
        }

        // debug helpers
        #if UNITY_EDITOR
        Dbg.AddToParent("Flowers", this);
        #endif
    }

    void OnEnable() {
        // try to replant any time we are enabled
        TryPlant();
    }

    // -- commands --
    /// add an owner for this flower
    [Server]
    public void Server_Grab() {
        m_IsFree = false;
    }

    /// remove an owner for this flower
    [Server]
    public void Server_Release() {
        m_IsFree = true;
    }

    /// move the flower to a position on the ground
    void TryPlant() {
        // wait until we're ready to plant
        if (m_Planting != Planting.Ready) {
            return;
        }

        var didHit = Physics.Raycast(
            m_Checkpoint.Position + m_Checkpoint.Forward * k_ForwardOffset + Vector3.up * k_UpOffset,
            Vector3.down,
            out var hit,
            k_RaycastLen,
            s_GroundMask,
            QueryTriggerInteraction.Ignore
        );

        if (!didHit) {
            return;
        }

        // move flower to the hit point
        transform.position = hit.point;

        // make the flower grow
        var targetScale = transform.localScale;
        transform.localScale = Vector3.Scale(transform.localScale, new Vector3(1, 0, 1));
        StartCoroutine(CoroutineHelpers.InterpolateByTime(m_SpawnTime, (k) => {
            transform.localScale = Vector3.Scale(targetScale, new Vector3(1, k * k, 1));
        }));

        // and mark it as planted
        m_Planting = Planting.Planted;

        // and let everyone know
        m_FlowerPlanted.Raise(this);
    }

    /// when the host toggles visbility
    [Server]
    public void Host_SetVisibility(bool isVisible) {
        if (isVisible) {
            TryPlant();
        }
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

    /// the flower's checkpoint
    public Checkpoint Checkpoint {
        get => m_Checkpoint;
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
        if (!k_MaterialCache.TryGetValue(key, out var material)) {
            material = Instantiate(m_Renderer.sharedMaterial);
            material.mainTexture = m_Texture;
            material.SetFloat("_Saturation", saturation);
            material.name = key;

            k_MaterialCache.Add(key, material);
        }

        return material;
    }

    // -- events --
    /// when the client receives the checkpoint
    void Client_OnCheckpointReceived(Checkpoint _p, Checkpoint _n) {
        m_Planting = Planting.Ready;
        TryPlant();
    }

    // -- factories --
    /// spawn a flower from a record
    [Server]
    public static CharacterFlower Server_Spawn(FlowerRec rec) {
        return Server_Spawn(
            rec.Key,
            rec.Pos,
            rec.Fwd
        );
    }

    /// spawn a flower from key and transform
    [Server]
    public static CharacterFlower Server_Spawn(
        CharacterKey key,
        Vector3 pos,
        Vector3 fwd
    ) {
        var prefab = CharacterDefs.Instance.Find(key)?.Flower;
        if (prefab == null) {
            Debug.LogError($"[flower] no prefab found for {key.Name()}");
            return null;
        }

        var flower = Instantiate(
            prefab,
            pos,
            Quaternion.identity
        );

        #if UNITY_EDITOR
        flower.name = $"Flower_{key.Name()}";
        #endif

        // store checkpoint info
        flower.m_Key = key;
        flower.m_Checkpoint = new Checkpoint(pos, fwd);

        // set initial state
        flower.m_IsFree = true;

        // plant the flower
        flower.m_Planting = Planting.Ready;
        flower.TryPlant();

        // spawn the game object for everyone
        NetworkServer.Spawn(flower.gameObject);

        return flower;
    }

    /// create state frame from this flower
    public CharacterState.Frame IntoState() {
        return m_Checkpoint.IntoState();
    }

    /// construct a record from this flower
    public FlowerRec IntoRecord() {
        return new FlowerRec(
            Key,
            m_Checkpoint.Position,
            m_Checkpoint.Forward
        );
    }
}