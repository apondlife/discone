using ThirdPerson;
using UnityEngine;
using UnityAtoms;
using UnityAtoms.BaseAtoms;

namespace Discone {

/// the discone (local) player; there is only one of these!
public sealed class Player: Player<InputFrame> {
    // -- state --
    [Header("state")]
    [Tooltip("the current player")]
    [SerializeField] DisconePlayerVariable m_Current;

    [Tooltip("the current player's character")]
    [SerializeField] DisconeCharacterVariable m_CurrentCharacter;

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

    [Tooltip("the distance to the far clip plane")]
    [SerializeField] FloatReference m_FarClipPlane;

    [Tooltip("if the player is closing their eyes (aspirational)")]
    [SerializeField] BoolVariable m_IsClosingEyes;

    // -- input --
    [Header("input")]
    [Tooltip("the currently controlled character")]
    [SerializeField] InputSource m_InputSource;

    // -- props --
    /// the checkpoint
    PlayerCheckpoint m_Checkpoint;

    /// a set of event subscriptions
    DisposeBag m_Subscriptions = new();

    // -- lifecycle --
    protected override void Awake() {
        base.Awake();

        // this is the current player
        m_Current.Value = this;

        // set deps
        m_Checkpoint = GetComponent<PlayerCheckpoint>();

        // bind events
        m_Subscriptions
            .Add(m_CurrentCharacter.ChangedWithHistory, OnDriveCharacter)
            .Add(m_IsDialogueActiveChanged, OnIsDialogueActiveChanged)
            .Add(m_Store.LoadFinished, OnStoreLoadFinished);
    }

    protected override void Update() {
        base.Update();

        // update global shader character pos
        var characterPos = Vector3.zero;

        // use the current character's position
        var character = m_CurrentCharacter.Value;
        if (character) {
            characterPos = character.Position;
        }

        Shader.SetGlobalVector(ShaderProps.CharacterPos, characterPos);

        // sync external state
        if (Checkpoint.IsSavingChanged) {
            m_IsClosingEyes.Value = Checkpoint.IsSaving;
        }
    }

    protected override void OnDestroy() {
        base.OnDestroy();

        // unbind events
        m_Subscriptions.Dispose();
    }

    // -- queries --
    /// the character
    public Character Character {
        get => m_CurrentCharacter.Value;
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
        // set ready on first drive
        if (!m_IsReady.Value) {
            m_IsReady.Value = true;
        }
    }

    /// when the dialog becomes in/active
    void OnIsDialogueActiveChanged(bool isDialogueActive) {
        InputSource.IsEnabled = !isDialogueActive;
    }

    // -- props/hot --
    /// if the input is enabled
    public bool IsInputEnabled {
        get => InputSource.IsEnabled;
        set => InputSource.IsEnabled = value;
    }

    // -- Player<InputFrame> --
    /// the character the player is currently driving
    public override PlayerInputSource<InputFrame> InputSource {
        get => m_InputSource;
    }
}

static class PlayerExt {
    /// if this game object is the current player
    public static bool IsLocalPlayer(this GameObject component) {
        return component.CompareTag("PlayerDialogueTarget");
    }

    /// if this component is the current player
    public static bool IsLocalPlayer(this Component component) {
        return component.CompareTag("PlayerDialogueTarget");
    }
}

}