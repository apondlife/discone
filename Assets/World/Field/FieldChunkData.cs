using UnityEngine;

public class FieldChunkData : ScriptableObject {
    [Tooltip("the custom terrain data, if any")]
    [SerializeField] public TerrainData TerrainData;

    [Tooltip("the material")]
    [SerializeField] public Material Material;
}