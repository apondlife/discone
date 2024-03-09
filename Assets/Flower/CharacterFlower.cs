using UnityEngine;
using Mirror;
using System.Collections.Generic;
using ThirdPerson;
using UnityAtoms;
using FMODUnity;
using Soil;

// TODO: rename to flower
/// a flower that a character leaves behind as its checkpoint
[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(StudioEventEmitter))]
public class CharacterFlower: NetworkBehaviour {
    // -- types --
    /// the flower's planting state
    enum Planting {
        Seed,
        Germ,
        Bloom
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

    /// the character layer mask
    static LayerMask s_CharacterMask;

    /// the wobble audio line
    static Musicker.Line s_Line = new(Musicker.Tone.I, Musicker.Quality.Maj7);

    // -- cfg --
    [Header("cfg")]
    [Tooltip("the texture to use for the flower")]
    [SerializeField] Texture m_Texture;

    [Tooltip("the saturation of the released flower")]
    [SerializeField] float m_Saturation = 0.8f;

    [Tooltip("the saturation of the released flower")]
    [SerializeField] float m_SpawnTime = 0.5f;

    // -- wobble --
    [Header("cfg/wobble")]
    [Tooltip("the wobble time when collided with")]
    [SerializeField] float m_WobbleTime = 0.5f;

    [Tooltip("the wobble frequency when collided with")]
    [SerializeField] float m_WobbleFrequency = 0.5f;

    [Tooltip("the wobble decay intensity when collided with")]
    [SerializeField] float m_WobbleDecay = 0.5f;

    [Tooltip("the wobble max amplitude when collided with")]
    [SerializeField] float m_WobbleAmplitude = 0.1f;

    // -- published --
    [Header("published")]
    [Tooltip("the event called when a flower gets planted")]
    [SerializeField] CharacterFlowerEvent m_FlowerPlanted;

    // -- refs --
    [Header("refs")]
    [Tooltip("the renderer for the flower")]
    [SerializeField] Renderer m_Renderer;

    [Tooltip("the FMOD emitter for the flower")]
    [SerializeField] StudioEventEmitter m_FmodEmitter;

    [Tooltip("the target game object for rescaling the flower")]
    [SerializeField] Transform m_ScaleTarget;

    // -- props --
    /// the assosciated character's key
    [SyncVar]
    CharacterKey m_Key;

    // the checkpoint this flower represents
    [SyncVar(hook = nameof(Client_OnCheckpointReceived))]
    Checkpoint m_Checkpoint;

    /// how many player's have grabbed this flower
    [SyncVar(hook = nameof(Client_OnGrabbingReceived))]
    int m_Grabbing = 0;

    /// if the flower has been planted
    Planting m_Planting = Planting.Seed;

    /// the number of characters touching the flower
    int m_Touching;

    /// the flower's initial scale
    Vector3 m_BaseScale = Vector3.one;

    /// the wobble animation
    Coroutine m_Wobble;

    // -- lifecycle
    void Awake() {
        m_Renderer.material = FindMaterial();

        // every byte counts
        if (s_GroundMask == 0) {
            s_GroundMask = LayerMask.GetMask("Default", "Field", "Indoor");
        }

        if (s_CharacterMask == 0) {
            s_CharacterMask = LayerMask.GetMask("Character");
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

    void OnTriggerEnter(Collider other) {
        // if its not a character, do nothing
        if (!s_CharacterMask.Contains(other.gameObject.layer)) {
            return;
        }

        // on character trigger enter, don't do anything if just planted
        if (m_Planting != Planting.Bloom) {
            return;
        }

        // track the number of character touching the flower
        m_Touching += 1;

        // stop existing wobble
        if (m_Wobble != null) {
            StopCoroutine(m_Wobble);
        }

        // start the wobble animation
        m_Wobble = StartCoroutine(CoroutineHelpers.InterpolateByTime(
            m_WobbleTime,
            (k) => {
                var scale = 1 + m_WobbleAmplitude * Mathf.Sin(m_WobbleFrequency * 2 * Mathf.PI * k) * (1 - Mathf.Pow(k, m_WobbleDecay));
                m_ScaleTarget.localScale = Vector3.Scale(
                    m_BaseScale,
                    Vector3.one * scale
                );
            },
            () => {
                m_Wobble = null;
            }
        ));

        // play sound
        m_FmodEmitter.Play();
        m_FmodEmitter.SetParameter("Tone", s_Line.Curr().Steps);

        // move through the line
        s_Line.Advance();
        if (Random.value < 0.5f) {
            s_Line.Advance();
        }
    }

    void OnTriggerExit(Collider other) {
        // if it's not a character, do nothing
        if (!s_CharacterMask.Contains(other.gameObject.layer)) {
            return;
        }

        // stop once no characters are touching the flower
        m_Touching = Mathf.Max(m_Touching - 1, 0);
        if (m_Touching == 0) {
            m_FmodEmitter.Stop();
        }
    }

    // -- commands --
    /// add an owner for this flower
    [Server]
    public void Server_Grab() {
        m_Grabbing += 1;
    }

    /// remove an owner for this flower
    [Server]
    public void Server_Release() {
        m_Grabbing -= 1;
    }

    /// move the flower to a position on the ground
    void TryPlant() {
        // wait until we're ready to plant
        if (m_Planting != Planting.Seed || m_Checkpoint == null) {
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
        var targetScale = m_BaseScale;
        m_ScaleTarget.localScale = Vector3.Scale(
            m_ScaleTarget.localScale,
            new Vector3(1.0f, 0.0f, 1.0f)
        );

        StartCoroutine(CoroutineHelpers.InterpolateByTime(
            m_SpawnTime,
            (k) => {
                m_ScaleTarget.localScale = Vector3.Scale(
                    targetScale,
                    new Vector3(1.0f, k * k, 1.0f)
                );
            },
            () => {
                m_Planting = Planting.Bloom;
            }
        ));

        // and move it to the next planting phase
        m_Planting = Planting.Germ;

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
    void Client_OnGrabbingReceived(int _prev, int _curr) {
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

    /// find cached material for texture and saturation
    Material FindMaterial() {
        return FindMaterial(m_Grabbing == 0 ? m_Saturation : 1.0f);
    }

    /// find cached material for texture and saturation
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
        // try and plant the flower
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
        flower.m_Grabbing = 0;

        // plant the flower
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