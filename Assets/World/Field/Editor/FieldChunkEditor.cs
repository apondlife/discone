using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FieldChunk))]
public class FieldChunkEditor: Editor {
    // -- constants --
    /// the parent dir
    private const string k_Parent = "Assets/World/Field";

    /// the chunk data dir
    private const string k_Dir = "Resources";

    // -- props --
    /// the character tunables
    private FieldChunk m_Chunk;

    // -- lifecycle --
    void OnEnable() {
        m_Chunk = serializedObject.targetObject as FieldChunk;
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        // draw create button
        // @2021.2: AssetDatabase.AssetPathToGUID(path, AssetPathToGUIDOptions.OnlyExistingAssets)
        GUI.enabled = AssetDatabase.AssetPathToGUID(AssetPath) == "";

        if (GUILayout.Button("Create Custom")) {
            CreateCustomTerrainData();
        }

        GUI.enabled = true;
    }

    // -- commands --
    /// create custom terrain data
    void CreateCustomTerrainData() {
        // create the dir, if necessary
        if (!AssetDatabase.IsValidFolder(Dir)) {
            AssetDatabase.CreateFolder(k_Parent, k_Dir);
        }

        // create the asset
        var custom = Instantiate(m_Chunk.TerrainData);
        AssetDatabase.CreateAsset(custom, AssetPath);
        m_Chunk.Reload();
    }

    // -- queries --
    /// path to the chunk dir
    string Dir {
        get => $"{k_Parent}/{k_Dir}";
    }

    /// path to the chunk asset
    string AssetPath {
        get => $"{Dir}/{m_Chunk.name}.asset";
    }
}
