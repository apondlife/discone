using ThirdPerson;
using UnityEngine;
using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine.InputSystem;

/// the discone (local) player; there is only one of these!
[RequireComponent(typeof(ThirdPerson.Player))]
public sealed class DisconePlayer: MonoBehaviour {
    // -- atoms --
    [Header("atoms")]
    [Tooltip("the current player")]
    [SerializeField] DisconePlayerVariable m_Current;

    [Tooltip("the current player")]
    [SerializeField] DisconeCharacterVariable m_Character;

    [Tooltip("the progress of the checkpoint save")]
    [SerializeField] FloatVariable m_SaveProgress;

    [Tooltip("the progress of the checkpoint load")]
    [SerializeField] FloatVariable m_LoadProgress;

    // -- events --
    [Header("events")]
    [Tooltip("if the dialogue is active")]
    [SerializeField] BoolEvent m_IsDialogueActiveChanged;

    // -- refs --
    [Header("refs")]
    [Tooltip("the input source")]
    [SerializeField] PlayerInputSource m_InputSource;

    [Tooltip("the distance to the far clip plane")]
    [SerializeField] FloatReference m_FarClipPlane;

    // -- props --
    /// a set of event subscriptions
    Subscriptions m_Subscriptions = new Subscriptions();

    // -- lifecycle --
    void Awake() {
        // this is the current player
        m_Current.Value = this;

        // bind events
        m_Subscriptions
            .Add(m_Character.ChangedWithHistory, OnDriveCharacter)
            .Add(m_IsDialogueActiveChanged, OnIsDialogueActiveChanged);

        // configure cameras
        var cameras = GetComponentsInChildren<UnityEngine.Camera>();
        foreach (var camera in cameras) {
            camera.farClipPlane = m_FarClipPlane.Value;
        }
    }

    void OnDestroy() {
        // unbind events
        m_Subscriptions.Dispose();
    }

    // -- events --
    // when the player starts driving a character
    void OnDriveCharacter(DisconeCharacterPair characters) {
        var prev = characters.Item2;
        prev?.OnRelease();

        var curr = characters.Item1;
        curr?.OnDrive();
    }

    /// when the dialog becomes in/active
    void OnIsDialogueActiveChanged(bool isDialogueActive) {
        m_InputSource.enabled = !isDialogueActive;
    }
}