using ThirdPerson;
using UnityEngine;
using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine.InputSystem;

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

    [Tooltip("the player's camera, if this player is the camera owner")]
    [SerializeField] GameObject m_PlayerCamera;

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
            .Add(m_IsDialogueActiveChanged, OnIsDialogueActiveChanged);

        // AAA: don't do this, create an input actions type of some kind
        m_InputSource.Bind(m_InputSource);
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

    // -- commands --
    public void Bind(InputActionAsset inputActionAsset, Transform look) {
        m_InputSource.Init(inputActionAsset, look);
    }

    /// give the camera to another player
    public void GiveCamera(Player player) {
        m_PlayerCamera = player.m_PlayerCamera;
        player.m_PlayerCamera = null;
    }

    // -- queries --
    /// the character
    public new Character Character {
        get => base.Character as Character;
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
    /// when the player starts driving a character
    void OnDriveCharacter(DisconeCharacterPair characters) {
        var prev = characters.Prev();

        // AAA: isReady standing in for the first player receiving the first character
        if (m_IsReady.Value && prev == Character) {
            return;
        }

        // set ready on first drive
        if (!m_IsReady.Value) {
            m_IsReady.Value = true;
        }

        // if we own the camera, toggle the character's virtual camera
        if (m_PlayerCamera) {
            if (prev) {
                prev.Camera.Toggle(false);
            }

            var next = characters.Next();
            if (next) {
                next.Camera.Toggle(true);
            }
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
    /// if this component is the current player
    public static bool IsLocalPlayer(this Component component) {
        return component.CompareTag("PlayerDialogueTarget");
    }
}

}