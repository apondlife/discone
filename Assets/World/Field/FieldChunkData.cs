using UnityEngine;

/// custom data for a field chunk
public class FieldChunkData: ScriptableObject {
    // -- fields --
    [Header("fields")]
    [Tooltip("the custom terrain data, if any")]
    [SerializeField] public TerrainData TerrainData;

    [Tooltip("the material")]
    [SerializeField] public Material Material;

    // -- queries --
    /// the name of the chunk asset for a named chunk
    public static string ChunkAssetName(string name) {
        return name;
    }

    /// the name of the terrain asset for a named chunk
    public static string TerrainAssetName(string name) {
        return $"{name}-terrain";
    }

    /// find saved chunk data for the named chunk, if any
    public static FieldChunkData Find(string name) {
        // load the custom chunk data, if it exists
        // TODO: cache this? (https://forum.unity.com/threads/does-unity-cache-results-of-resources-load.270861/)
        var data = Resources.Load<FieldChunkData>(ChunkAssetName(name));
        if (data == null) {
            return null;
        }

        // we may need to repair some old chunks that didn't have data saved independently
        if (data.TerrainData == null) {
            data.TerrainData = Resources.Load<TerrainData>(TerrainAssetName(name));
        }

        return data;
    }
}