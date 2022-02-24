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
    /// the field chunk
    private FieldChunk m_Chunk;

    // -- lifecycle --
    void OnEnable() {
        m_Chunk = serializedObject.targetObject as FieldChunk;
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        if (GUILayout.Button("Create Custom")) {
            CreateCustomChunkData();
        }
    }

    // -- commands --
    /// create custom terrain data
    void CreateCustomChunkData() {
        // create the dir, if necessary
        if (!AssetDatabase.IsValidFolder(Dir)) {
            AssetDatabase.CreateFolder(k_Parent, k_Dir);
        }

        // create the asset
        var asset = ScriptableObject.CreateInstance<FieldChunkData>();
        asset.Material = m_Chunk.Material;
        asset.TerrainData = Instantiate(m_Chunk.TerrainData) as TerrainData;
        asset.TerrainData.name = $"{m_Chunk.name}-data";

        AssetDatabase.CreateAsset(asset, AssetPath);

        // reload the chunk
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
