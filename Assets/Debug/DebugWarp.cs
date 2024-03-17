using Cinemachine;
using Soil;
using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Serialization;

namespace Discone {

/// the debug warping
sealed class DebugWarp: MonoBehaviour {
    // -- cfg --
    [Header("cfg")]
    [FormerlySerializedAs("m_WarpTag")]
    [Tooltip("the warp tag")]
    [TagField]
    [SerializeField] string m_Tag;

    [FormerlySerializedAs("m_WarpRepeat")]
    [Tooltip("the timer between repeat warp taps")]
    [SerializeField] EaseTimer m_Repeat;

    // -- input --
    [Header("input")]
    [Tooltip("the warp next action")]
    [SerializeField] InputActionReference m_Warp;

    [Tooltip("the warp to index action")]
    [SerializeField] InputActionReference m_WarpIndex;

    // -- refs --
    [Header("refs")]
    [Tooltip("the debug camera")]
    [SerializeField] DebugCamera m_Camera;

    [Tooltip("the current character")]
    [SerializeField] DisconeCharacterVariable m_CurrentCharacter;

    [Tooltip("the search query for the initial warp point")]
    [SerializeField] StringVariable m_StartQuery;

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
        m_WarpPoints = new Ring<GameObject>(GameObject.FindGameObjectsWithTag(m_Tag));

        // bind events
        m_Subscriptions
            .Add(m_Warp, OnWarpPressed)
            .Add(m_WarpIndex, OnWarpIndexPressed)
            .Add(m_Input.SpawnCharacter, OnSpawnCharacterPressed);

        #if UNITY_EDITOR
        m_Subscriptions
            .Add(m_CurrentCharacter.ChangedWithHistory, OnCharacterChanged);
        #endif
    }

    void OnDestroy() {
        m_Subscriptions.Dispose();
    }

    void Update() {
        m_Repeat.Tick();
    }

    // -- commands --
    /// move to the next warp point (or move the camera if in noclip)
    void Warp() {
        if (m_Repeat.IsActive) {
            m_WarpPoints.Offset();
        }

        Warp(m_WarpPoints.Head);
        m_Repeat.Start();
    }

    /// move to the warp point (or move the camera if in noclip)
    void Warp(GameObject warpPoint) {
        var warpPos = warpPoint.transform.position;
        if (m_Camera.IsNoClip) {
            m_Camera.transform.position = warpPos;
        } else {
            MoveCharacterToPosition(warpPos);
        }
    }

    /// if a query is set, try to warp to the start point
    #if UNITY_EDITOR
    void WarpToStartPoint() {
        var query = m_StartQuery.Value;
        if (string.IsNullOrEmpty(query)) {
            return;
        }

        var match = null as GameObject;
        foreach (var warpPoint in m_WarpPoints) {
            if (!warpPoint.name.Contains(query)) {
                match = warpPoint;
                break;
            }
        }

        if (!match) {
            Log.Debug.E($"failed to match warp point for {query}");
            return;
        }

        // TODO: every stateful sequence (Dream, Intro) needs to be able to tear itself
        // down if the character is not present
        Log.Debug.I($"starting @ {match.name}");
        Warp(match);
    }
    #endif

    /// move the current character the camera position
    void MoveCharacterToDebugCamera() {
        MoveCharacterToPosition(m_Camera.transform.position);
    }

    /// move the current character to position
    void MoveCharacterToPosition(Vector3 position) {
        var character = m_CurrentCharacter.Value;

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

    #if UNITY_EDITOR
    // TODO: an event when the game is initialized (the first character exists)
    /// when the initial character is set
    void OnCharacterChanged(DisconeCharacterPair characters) {
        if (characters.Item2) {
            return;
        }

        #if UNITY_EDITOR
        WarpToStartPoint();
        #endif
    }
    #endif
}

}