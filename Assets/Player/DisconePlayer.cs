using ThirdPerson;
using UnityEngine;
using UnityAtoms;
using UnityAtoms.BaseAtoms;

namespace Discone {

/// the discone (local) player; there is only one of these!
[RequireComponent(typeof(ThirdPerson.Player))]
public sealed class DisconePlayer: MonoBehaviour {
    // -- state --
    [Header("state")]
    [Tooltip("the current player")]
    [SerializeField] DisconePlayerVariable m_Current;

    [Tooltip("the current player")]
    [SerializeField] DisconeCharacterVariable m_Character;

    [Tooltip("if this player is ready with a character")]
    [SerializeField] BoolVariable m_IsReady;

    // -- subscribed --
    [Header("subscribed")]
    [Tooltip("if the dialogue is active")]
    [SerializeField] BoolEvent m_IsDialogueActiveChanged;

    // -- refs --
    [Header("refs")]
    [Tooltip("the persistence store")]
    [SerializeField] Store m_Store;

    [Tooltip("the input source")]
    [SerializeField] PlayerInputSource m_InputSource;

    [Tooltip("the distance to the far clip plane")]
    [SerializeField] FloatReference m_FarClipPlane;

    // -- props --
    /// the checkpoint
    PlayerCheckpoint m_Checkpoint;

    /// a set of event subscriptions
    DisposeBag m_Subscriptions = new DisposeBag();

    // -- lifecycle --
    void Awake() {
        // this is the current player
        m_Current.Value = this;

        // set deps
        m_Checkpoint = GetComponent<PlayerCheckpoint>();

        // bind events
        m_Subscriptions
            .Add(m_Character.ChangedWithHistory, OnDriveCharacter)
            .Add(m_IsDialogueActiveChanged, OnIsDialogueActiveChanged)
            .Add(m_Store.LoadFinished, OnStoreLoadFinished);
    }

    void Update() {
        // update global shader character pos
        var characterPos = Vector3.zero;

        // use the current character's position
        var character = m_Character.Value;
        if (character != null) {
            characterPos = character.Position;
        }

        Shader.SetGlobalVector(ShaderProps.CharacterPos, characterPos);
    }

    void OnDestroy() {
        // unbind events
        m_Subscriptions.Dispose();
    }

    // -- queries --
    /// the character
    public DisconeCharacter Character {
        get => m_Character.Value;
    }

    /// the checkpoint
    public PlayerCheckpoint Checkpoint {
        get => m_Checkpoint;
    }

    /// an event when player is ready with a character
    public BoolEvent IsReadyChanged {
        get => m_IsReady.Changed;
    }

    // -- events --
    /// when the store loads
    void OnStoreLoadFinished() {
        // TODO: implement this
        // if the store has a player & character, spawn that character remotely
        // --> drive that character
        // otherwise, find the first available character (OnlinePlayer.DriveInitialCharacter)
        // --> drive that character
    }

    /// when the player starts driving a character
    void OnDriveCharacter(DisconeCharacterPair characters) {
        var prev = characters.Item2;
        prev?.OnRelease();

        var curr = characters.Item1;
        curr?.OnDrive();

        // set ready on first drive
        if (!m_IsReady.Value) {
            m_IsReady.Value = true;
        }
    }

    /// when the dialog becomes in/active
    void OnIsDialogueActiveChanged(bool isDialogueActive) {
        m_InputSource.enabled = !isDialogueActive;
    }
}

}