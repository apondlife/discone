using UnityEngine;
using UnityAtoms.Discone;

/// changes the skybox color based on region
public class SkyRegionColor: MonoBehaviour {
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

    /// a list of event subscriptions
    Subscriptions m_Subscriptions = new Subscriptions();

    // -- lifecycle --
    void Awake() {
        m_Material = RenderSettings.skybox.Unsaved();
        RenderSettings.skybox = m_Material;
    }

    void Start() {
        // bind events
        m_Subscriptions
            .Add(m_RegionEntered, OnRegionEntered);
    }

    void OnDestroy() {
        // unbind events
        m_Subscriptions.Dispose();
    }

    // -- events --
    /// when the region changes
    void OnRegionEntered(Region region) {
        var exposureBg = m_Material.GetFloat("_ExposureBackground");
        var exposureFg = m_Material.GetFloat("_ExposureForeground");
        var background = m_Material.GetColor("_Background");
        var foreground = m_Material.GetColor("_Foreground");

        StartCoroutine(CoroutineHelpers.InterpolateByTime(m_ChangeDuration, (k) => {
            m_Material.SetFloat(
                "_ExposureBackground",
                Mathf.Lerp(exposureBg, region.SkyboxExposureBackground, k)
            );

            m_Material.SetFloat(
                "_ExposureForeground",
                Mathf.Lerp(exposureFg, region.SkyboxExposureForeground, k)
            );

            m_Material.SetColor(
                "_Background",
                Color.Lerp(background, region.SkyboxColorBackground, k)
            );

            m_Material.SetColor(
                "_Foreground",
                Color.Lerp(foreground, region.SkyboxColorForeground, k)
            );
        }));
    }
}
