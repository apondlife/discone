using UnityEngine;

[System.Serializable]
public class Region {
    public string DisplayName;

    [Header("FMOD")]
    public string MusicString;

    [Header("Skybox Stuff")]
    public Color SkyboxColorForeground;
    public Color SkyboxColorBackground;
    [Range(0.0f, 8.0f)]
    public float SkyboxExposure;
    public Color SkyboxFog;
}