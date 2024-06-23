using System;
using ThirdPerson;
using UnityEngine;
using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using CharacterEvent = UnityAtoms.CharacterEvent;

namespace Discone {

/// the discone (local) player; there is only one of these!
public sealed class Player: Player<InputFrame> {
    // -- state --
    [Header("state")]
    [Tooltip("the current player")]
    [SerializeField] PlayerVariable m_Current;

    [Tooltip("the current player's character")]
    [SerializeField] CharacterVariable m_CurrentCharacter;

    [Tooltip("if this player is ready with a character")]
    [SerializeField] BoolVariable m_IsReady;

    [Tooltip("the player's camera, if this player is the camera owner")]
    [SerializeField] GameObject m_PlayerCamera;

    // -- subscribed --
    [Header("subscribed")]
    [Tooltip("if the dialogue is active")]
    [SerializeField] BoolEvent m_IsDialogueActiveChanged;

    [FormerlySerializedAs("m_SwitchCharacter")]
    [Tooltip("drive this player's character")]
    [SerializeField] CharacterEventInstancer m_DriveCharacter;

    [Tooltip("warps the local player to a location")]
    [SerializeField] PlacementEvent m_Player_Warp;

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

    /// the initial checkpoint to use, if any
    Placement m_InitialCheckpoint;

    /// a set of event subscriptions
    DisposeBag m_Subscriptions = new();

    // -- lifecycle --
    protected override void Awake() {
        base.Awake();

        // destroy any unowned player cameras
        if (m_PlayerCamera && !m_PlayerCamera.activeSelf) {
            Destroy(m_PlayerCamera.gameObject);
            m_PlayerCamera = null;
        }

        // this is the current player
        if (m_PlayerCamera) {
            m_Current.Value = this;
        }

        // set deps
        m_Checkpoint = GetComponent<PlayerCheckpoint>();

        // bind events
        m_Subscriptions
            .Add(m_DriveCharacter.Event, OnDriveCharacter)
            .Add(m_IsDialogueActiveChanged, OnIsDialogueActiveChanged)
            .Add(m_Player_Warp, OnWarp);
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
    /// bind the input to the player
    public void Bind(Input input) {
        m_InputSource.Bind(input);
    }

    /// start the character at the given checkpoint
    public void StartFromCheckpoint(Placement checkpoint) {
        m_InitialCheckpoint = checkpoint;
    }

    /// give the camera to another player
    public void GiveCamera(Player player) {
        if (!m_PlayerCamera) {
            Log.Player.E($"tried to give camera, but didn't own the camera");
        }

        var playerCamera = m_PlayerCamera;

        // swap camera control
        player.m_PlayerCamera = playerCamera;
        m_PlayerCamera = null;
        playerCamera.transform.parent = player.transform;

        // update the current player/character
        m_Current.Value = player;
        m_CurrentCharacter.Value = player.Character;
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

    /// an event to switch this player's character
    public CharacterEvent DriveCharacter {
        get => m_DriveCharacter.Event;
    }

    // -- events --
    /// when the player should drive a new character
    void OnDriveCharacter(Character next) {
        var prev = Character;

        // drive the new character
        Drive(next);

        // if we have an initial checkpoint, try to plant (or smell) a flower there
        if (!prev && m_InitialCheckpoint != null) {
            next.PlantFlower(m_InitialCheckpoint);
            m_InitialCheckpoint = null;
        }

        // if we're the current player
        if (m_Current.Value == this) {
            // set ready on first drive
            if (!m_IsReady.Value) {
                m_IsReady.Value = true;
            }

            // update the current character
            m_CurrentCharacter.Value = next;

            // toggle the character's virtual camera
            if (prev) {
                prev.Camera.Toggle(false);
            }

            if (next) {
                next.Camera.Toggle(true);
            }
        }
    }

    /// when the dialog becomes in/active
    void OnIsDialogueActiveChanged(bool isDialogueActive) {
        InputSource.IsEnabled = !isDialogueActive;
    }

    /// warps the local player to a location
    void OnWarp(Placement placement) {
        var character = Character;

        var nextState = character.State.Curr.Copy();
        nextState.Position = placement.Position;
        nextState.Forward = placement.Forward;

        character.ForceState(nextState);
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