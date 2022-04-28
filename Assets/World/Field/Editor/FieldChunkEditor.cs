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

        // draw spacing
        GUILayout.Space(10.0f);

        // draw label
        var style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontStyle = FontStyle.Bold;
        GUILayout.Label("custom chunk", style);

        // draw button
        if (GUILayout.Button("create or update")) {
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

        // find or init chunk data
        var chunkData = m_Chunk.CustomData;
        if (chunkData == null) {
            chunkData = ScriptableObject.CreateInstance<FieldChunkData>();
        }

        // find or init terrain data
        var terrainData = chunkData?.TerrainData;
        if (terrainData == null) {
            terrainData = Instantiate(m_Chunk.TerrainData) as TerrainData;
            terrainData.name = $"{m_Chunk.name}-data";
        }

        // update chunk data
        chunkData.Material = m_Chunk.Material;
        chunkData.TerrainData = terrainData;

        // create or update assets
        CreateOrUpdateAsset(terrainData, TerrainAssetPath);
        CreateOrUpdateAsset(chunkData, ChunkAssetPath);

        // reload the chunk
        m_Chunk.Reload();
    }

    /// create or update an asset at the given path
    void CreateOrUpdateAsset(Object obj, string path) {
        var id = AssetDatabase.AssetPathToGUID(
            path,
            AssetPathToGUIDOptions.OnlyExistingAssets
        );

        if (id == null || id == "") {
            AssetDatabase.CreateAsset(obj, path);
        } else {
            AssetDatabase.SaveAssetIfDirty(obj);
        }
    }

    // -- queries --
    /// path to the chunk dir
    string Dir {
        get => $"{k_Parent}/{k_Dir}";
    }

    /// path to the chunk asset
    string ChunkAssetPath {
        get => $"{Dir}/{FieldChunkData.ChunkAssetName(m_Chunk.name)}.asset";
    }

    /// path to the terrain data asset
    string TerrainAssetPath {
        get => $"{Dir}/{FieldChunkData.TerrainAssetName(m_Chunk.name)}.asset";
    }
}
