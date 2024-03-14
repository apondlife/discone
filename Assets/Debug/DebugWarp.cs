using Cinemachine;
using Soil;
using UnityAtoms;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Discone {

/// the debug warping
sealed class DebugWarp: MonoBehaviour {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the warp tag")]
    [TagField]
    [SerializeField] string m_WarpTag;

    [Tooltip("the timer between repeat warp taps")]
    [SerializeField] EaseTimer m_WarpRepeat;

    [Tooltip("the warp next action")]
    [SerializeField] InputActionReference m_Warp;

    [Tooltip("the warp to index action")]
    [SerializeField] InputActionReference m_WarpIndex;

    // -- refs --
    [Header("refs")]
    [Tooltip("the current character")]
    [SerializeField] DisconePlayerVariable m_Player;

    [Tooltip("the current character")]
    [SerializeField] DebugCamera m_Camera;

    // -- props --
    /// the input
    DebugInput m_Input;

    /// the list of warp points
    Ring<GameObject> m_WarpPoints;

    /// the subscriptions
    readonly DisposeBag m_Subscriptions = new();

    // -- lifecycle --
    void Awake() {
        // get dependencies
        m_Input = GetComponentInParent<DebugInput>();
        m_WarpPoints = new Ring<GameObject>(GameObject.FindGameObjectsWithTag(m_WarpTag));

        // bind events
        m_Subscriptions
            .Add(m_Warp, OnWarpPressed)
            .Add(m_WarpIndex, OnWarpIndexPressed)
            .Add(m_Input.SpawnCharacter, OnSpawnCharacterPressed);
    }

    void OnDestroy() {
        m_Subscriptions.Dispose();
    }

    void Update() {
        m_WarpRepeat.Tick();
    }

    // -- commands --
    /// move the camera to the next warp point
    void Warp() {
        if (m_WarpRepeat.IsActive) {
            m_WarpPoints.Offset();
        }

        // teleport camera if in noclip, otherwise character
        var warpPos = m_WarpPoints.Head.transform.position;
        if (m_Camera.IsNoClip) {
            m_Camera.transform.position = warpPos;
        } else {
            MoveCharacterToPosition(warpPos);
        }

        m_WarpRepeat.Start();
    }

    /// move the current character the camera position
    void MoveCharacterToDebugCamera() {
        MoveCharacterToPosition(m_Camera.transform.position);
    }

    /// move the current character to position
    void MoveCharacterToPosition(Vector3 position) {
        var character = m_Player.Value.Character;

        // build frame at position
        var nextFrame = character.State.Curr.Copy();
        nextFrame.Position = position;
        nextFrame.Velocity = Vector3.zero;

        // force to new position
        character.ForceState(nextFrame);
    }

    // -- events --
    /// .
    void OnWarpPressed(InputAction.CallbackContext _) {
        Warp();
    }

    /// .
    void OnWarpIndexPressed(InputAction.CallbackContext ctx) {
        if (ctx.control is not KeyControl k) {
            return;
        }

        m_WarpPoints.Move((int)k.keyCode - (int)KeyCode.Alpha1);
        Warp();
    }

    /// .
    void OnSpawnCharacterPressed(InputAction.CallbackContext _) {
        if (m_Camera.IsNoClip) {
            MoveCharacterToDebugCamera();
        }
    }
}

}