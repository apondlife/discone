using UnityEngine;
using UnityAtoms;

#if UNITY_EDITOR
/// debug utilties
public class DebugOptions: MonoBehaviour {
    // -- module --
    /// the shared instance
    public static DebugOptions s_Instance;

    /// resolve the module
    public static DebugOptions Get {
        get => s_Instance;
    }

    // -- character --
    [Header("character")]
    [Tooltip("the character to spawn at the editor camera position")]
    [SerializeField] CharacterKey m_CharacterKey;

    // -- refs --
    [Header("refs")]
    [Tooltip("the entity repos")]
    [SerializeField] EntitiesVariable m_Entites;

    // -- lifecycle --
    void Awake() {
        // store singleton
        if (s_Instance == null) {
            s_Instance = this;
        } else {
            Destroy(this);
        }
    }

    // -- commands --
    [ContextMenu("Spawn Character")]
    public void SpawnCharacter() {
        // find the current player
        var player = m_Entites.Value
            .Players
            .Current;

        if (player == null) {
            Debug.LogError($"[debug] no player to spawn debug character");
            return;
        }

        // spawn a new character
        var character = CreateCharacter();
        player.Command_DriveSpawnedCharacter(character);
    }

    // -- factories --
    /// create debug character, if enabled
    CharacterRec CreateCharacter() {
        // get the editor camera
        var scene = UnityEditor.SceneView.lastActiveSceneView;
        if (scene == null) {
            return null;
        }

        var camera = scene.camera;
        if (camera == null) {
            return null;
        }

        // get the look position and direction
        var ct = camera.transform;
        var pos = ct.position;
        var fwd = ct.forward;

        // create a debug character rec
        var character = new CharacterRec(
            m_CharacterKey,
            pos,
            Quaternion.LookRotation(
                Vector3.ProjectOnPlane(fwd, Vector3.up),
                Vector3.up
            ),
            null
        );

        return character;
    }
}
#endif