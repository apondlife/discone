using UnityEngine;
using UnityAtoms;

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
    public void SpawnCharacter(Transform t) {
        // find the current player
        var player = m_Entites.Value
            .Players
            .Current;

        if (player == null) {
            Debug.LogError($"[debug] no player to spawn debug character");
            return;
        }

        // spawn a new character
        var character = CreateCharacter(t);
        player.Command_DriveSpawnedCharacter(character);
    }

    [ContextMenu("Spawn Character at Scene Camera")]
    public CharacterRec SpawnCharacterAtSceneView() {
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
        return CreateCharacter(camera.transform);
    }

    // -- factories --
    /// create debug character, if enabled
    CharacterRec CreateCharacter(Transform t) {

        var pos = t.position;
        var fwd = t.forward;

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