using UnityEngine;

[System.Serializable]
public class Region {
    public string DisplayName;

    [Header("FMOD")]
    public string MusicString;

    [Header("Skybox Stuff")]
    public Color SkyboxColorForeground;

    [UnityEngine.Serialization.FormerlySerializedAs("SkyboxExposure")]
    [Range(0.0f, 8.0f)]
    public float SkyboxExposureForeground;

    public Color SkyboxColorBackground;

    [UnityEngine.Serialization.FormerlySerializedAs("SkyboxExposure")]
    [Range(0.0f, 8.0f)]
    public float SkyboxExposureBackground;

    public Color SkyboxFog;
}