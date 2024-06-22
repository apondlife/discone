using ThirdPerson;
using UnityAtoms;
using UnityEngine;
using UnityEngine.Serialization;
using CharacterEvent = UnityAtoms.CharacterEvent;

namespace Discone {

/// a character
[RequireComponent(typeof(CharacterCheckpoint))]
[RequireComponent(typeof(CharacterWrap))]
[RequireComponent(typeof(Character_Online))]
public sealed class Character: Character<InputFrame> {
    // -- id --
    [Header("id")]
    [Tooltip("the character's key")]
    [SerializeField] CharacterKey m_Key;

    // -- cfg --
    [Header("published")]
    [Tooltip("the character spawning event")]
    [FormerlySerializedAs("m_Spawned")]
    [SerializeField] CharacterEvent m_SpawnedCharacter;

    [Tooltip("the character being destroyed event")]
    [FormerlySerializedAs("m_Destroyed")]
    [SerializeField] CharacterEvent m_DestroyedCharacter;

    // -- props --
    /// the music
    CharacterMusicBase m_Music;

    /// the dialogue
    CharacterDialogue m_Dialogue;

    /// the checkpoint spawner
    CharacterCheckpoint m_Checkpoint;

    /// the online character
    Character_Online m_Online;

    /// the trigger collider
    Collider m_Collider;

    // -- lifecycle --
    protected override void Awake() {
        base.Awake();

        // set props
        // TODO: don't get component
        m_Music = GetComponentInChildren<CharacterMusicBase>(true);
        m_Dialogue = GetComponentInChildren<CharacterDialogue>(true);
        m_Checkpoint = GetComponent<CharacterCheckpoint>();
        m_Collider = GetComponent<Collider>();
        m_Online = GetComponent<Character_Online>();

        // debug
        #if UNITY_EDITOR
        Dbg.AddToParent("Characters", this);
        #endif
    }

    protected override void Start() {
        base.Start();

        // send spawned event
        m_SpawnedCharacter.Raise(this);
    }

    protected override void OnDestroy() {
        base.OnDestroy();

        // send destroyed event
        m_DestroyedCharacter.Raise(this);
    }

    // -- commands --
    /// plant a flower at a checkpoint
    public void PlantFlower(Checkpoint checkpoint) {
        m_Checkpoint.Create(checkpoint);
    }

    // -- queries --
    /// the character's key
    public CharacterKey Key {
        get => m_Key;
    }

    /// the character's position
    public Vector3 Position {
        get => State.Curr.Position;
    }

    /// the music
    public CharacterMusicBase Music {
        get => m_Music;
    }

    /// the online character
    public Character_Online Online {
        get => m_Online;
    }

    /// the character dialogue
    public CharacterDialogue Dialogue {
        get => m_Dialogue;
    }

    /// the checkpoint spawner
    public CharacterCheckpoint Checkpoint {
        get => m_Checkpoint;
    }

    /// the character's flower
    public CharacterFlower Flower {
        get => m_Checkpoint.Flower;
    }

    /// the character's trigger collider
    public Collider Collider {
        get => m_Collider;
    }

    // -- factories --
    /// instantiate a rec from a character
    public CharacterRec IntoRecord() {
        // if the position is zero, don't save this record
        // HACK: bit of a hack. not sure why the remote state get set to zero in
        // some situations, like when shutting down immediately after disconnect.
        // mirror zero-ing out sync vars for some reason?
        var pos = State.Curr.Position;
        if (pos == Vector3.zero) {
            Debug.LogWarning($"[chrctr] {name} - tried to save character w/ a zero-position");
            return null;
        }

        return new CharacterRec(
            Key,
            pos,
            State.Curr.LookRotation,
            m_Checkpoint.IntoRecord()
        );
    }
}

}