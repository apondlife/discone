using UnityEngine;
using UnityAtoms.Discone;

/// the current skybox region
public class SkyRegionColor : MonoBehaviour {
    // -- tunables --
    [Header("tunables")]
    [Tooltip("the transition duration between region colors")]
    [SerializeField] float m_ChangeDuration;

    // -- events --
    [Header("events")]
    [Tooltip("the transition duration between region colors")]
    [SerializeField] RegionEvent m_RegionEntered;

    // -- props --
    /// a reference to the shared skybox material
    Material m_Material;

    /// the number of seconds elapsed on the current transition
    float m_Elapsed;

    /// the color we are interpolating from
    SkyColor m_SrcColor;

    /// the color we are interpolating to
    SkyColor m_DstColor;

    /// a list of event subscriptions
    Subscriptions m_Subscriptions = new Subscriptions();

    // -- lifecycle --
    void Awake() {
        m_Material = RenderSettings.skybox.Unsaved();
        RenderSettings.skybox = m_Material;
    }

    void Start() {
        // bind events
        m_Subscriptions.Add(m_RegionEntered, OnRegionEntered);
    }

    void Update() {
        if (m_DstColor == null) {
            return;
        }

        // get interpolation pct
        var k = m_Elapsed / m_ChangeDuration;

        var foreground = Color.Lerp(
            m_SrcColor.Foreground,
            m_DstColor.Foreground,
            k
        );

        var fgExposure = Mathf.Lerp(
            m_SrcColor.ForegroundExposure,
            m_DstColor.ForegroundExposure,
            k
        );

        var background = Color.Lerp(
            m_SrcColor.Background,
            m_DstColor.Background,
            k
        );

        var bgExposure = Mathf.Lerp(
            m_SrcColor.ForegroundExposure,
            m_DstColor.ForegroundExposure,
            k
        );

        // update material uniforms
        m_Material.SetColor("_Foreground", foreground);
        m_Material.SetFloat("_ExposureForeground", fgExposure);
        m_Material.SetColor("_Background", background);
        m_Material.SetFloat("_ExposureBackground", bgExposure);

        // if we reach the destination, this is the last frame
        if (m_Elapsed >= m_ChangeDuration) {
            m_DstColor = null;
        }

        // update elapsed time
        m_Elapsed = Mathf.Min(m_Elapsed + Time.deltaTime, m_ChangeDuration);
    }

    void OnDestroy() {
        // unbind events
        m_Subscriptions.Dispose();
    }

    // -- events --
    /// when the region changes
    void OnRegionEntered(Region region) {
        // if this is the first region, set the color directly
        if (m_SrcColor == null) {
            m_Elapsed = 1.0f;
            m_DstColor = region.SkyColor;
            m_SrcColor = region.SkyColor;
        }
        // otherwise, interpolate from current color to region color
        else {
            m_Elapsed = 0.0f;
            m_DstColor = region.SkyColor;
            m_SrcColor = new SkyColor(
                m_Material.GetColor("_Foreground"),
                m_Material.GetFloat("_ExposureForeground"),
                m_Material.GetColor("_Background"),
                m_Material.GetFloat("_ExposureBackground")
            );
        }
    }
}
