using UnityEngine;
using UnityAtoms.Discone;
using ThirdPerson;

namespace Discone {

/// the current skybox region
public class RegionAtmosphere: MonoBehaviour {
    // -- tunables --
    [Header("tunables")]
    [Tooltip("the transition timer between region colors")]
    [SerializeField] EaseTimer m_Timer;

    // -- events --
    [Header("events")]
    [Tooltip("the transition duration between region colors")]
    [SerializeField] RegionEvent m_RegionEntered;

    // -- props --
    /// a reference to the shared skybox material
    Material m_Material;

    /// the region we are interpolating ambience from
    Region m_SrcRegion;

    /// the region we are interpolating ambience to
    Region m_DstRegion;

    /// the interpolated region
    Region m_CurrRegion;

    /// a list of event subscriptions
    DisposeBag m_Subscriptions = new DisposeBag();

    // -- lifecycle --
    void Awake() {
        m_Material = RenderSettings.skybox.Unsaved();
        RenderSettings.skybox = m_Material;

        // set props
        m_CurrRegion = new Region();
    }

    void Start() {
        // bind events
        m_Subscriptions.Add(m_RegionEntered, OnRegionEntered);
    }

    void Update() {
        if (!m_Timer.IsActive) {
            return;
        }

        m_Timer.Tick();

        // get interpolation pct
        Region.Lerp(
            ref m_CurrRegion,
            m_SrcRegion,
            m_DstRegion,
            m_Timer.Pct
        );

        Render(m_CurrRegion);
    }

    void OnDestroy() {
        // unbind events
        m_Subscriptions.Dispose();
    }

    // -- commands --
    void Render(Region region) {
        // update sky material color
        var color = region.SkyColor;
        m_Material.SetColor(ShaderProps.Foreground, color.Foreground);
        m_Material.SetFloat(ShaderProps.ForegroundExposure, color.ForegroundExposure);
        m_Material.SetColor(ShaderProps.Background, color.Background);
        m_Material.SetFloat(ShaderProps.BackgroundExposure, color.BackgroundExposure);

        // update fog settings
        var fog = region.Fog;
        RenderSettings.fogColor = fog.Color;
        RenderSettings.fogEndDistance = fog.EndDistance;
        RenderSettings.fogStartDistance = fog.StartDistance;
    }

    // -- events --
    /// when the region changes
    void OnRegionEntered(Region region) {
        // if this is the first region, set everything directly
        if (m_SrcRegion == null) {
            m_SrcRegion = region;
            m_DstRegion = region;
            Render(m_DstRegion);
        }
        // otherwise, interpolate from current region tonew region
        else {
            m_SrcRegion = m_CurrRegion.Copy();
            m_DstRegion = region;
            m_Timer.Start();
        }
    }
}

}