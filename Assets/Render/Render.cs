using UnityEngine;

namespace Discone {

/// the rendering pipeline
[ExecuteAlways]
sealed class Render: MonoBehaviour {
    // -- data --
    [Header("data")]
    [Tooltip("a small value shared by all shaders")]
    [SerializeField] float m_Epsilon;

    [Tooltip("the angle that is considered to be a wall")]
    [SerializeField] float m_WallAngle;

    // -- light --
    [Header("light")]
    [Tooltip("the reflected light intensity relative to the directional light")]
    [SerializeField] float m_ReflectedLightIntensity;

    [Tooltip("the ambient light intensity relative to the directional light")]
    [SerializeField] float m_AmbientLightIntensity;

    // -- lifecycle --
    void Start() {
        SyncUniforms();
    }

    void Update() {
        // in editor mode, use camera position
        #if UNITY_EDITOR
        if (Application.isPlaying) {
            var camera = EditorCamera.Get;
            if (camera != null) {
                Shader.SetGlobalVector(
                    ShaderProps.CharacterPos,
                    camera.transform.position
                );
            }
        }
        #endif
    }

    void OnValidate() {
        SyncUniforms();
    }

    // -- commands --
    /// update global shader uniforms
    void SyncUniforms() {
        Shader.SetGlobalFloat(
            ShaderProps.WallAngle,
            m_WallAngle
        );

        Shader.SetGlobalFloat(
            ShaderProps.Epsilon,
            m_Epsilon
        );

        Shader.SetGlobalFloat(
            ShaderProps.AmbientLightIntensity,
            m_AmbientLightIntensity
        );

        Shader.SetGlobalFloat(
            ShaderProps.ReflectedLightIntensity,
            m_ReflectedLightIntensity
        );
    }
}

}