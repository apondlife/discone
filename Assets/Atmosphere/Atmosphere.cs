using Soil;
using UnityEngine;
using UnityAtoms.Discone;

namespace Discone {

// BUG: if you start in no region, there's no atmosphere
// TODO: stop this from thrashing git state
/// the current skybox region
public partial class Atmosphere: MonoBehaviour {
    // -- state --
    [Header("state")]
    [Tooltip("the interpolated region")]
    [SerializeField] Region m_CurrRegion;

    // -- tuning --
    [Header("tuning")]
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

    /// a list of event subscriptions
    DisposeBag m_Subscriptions = new();

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

        Render();
    }

    void OnDestroy() {
        // unbind events
        m_Subscriptions.Dispose();
    }

    // -- commands --
    /// render the current region
    void Render() {
        var region = m_CurrRegion;

        // update sky material color
        var color = region.Sky;
        m_Material.SetColor(ShaderProps.Foreground, color.Foreground);
        m_Material.SetFloat(ShaderProps.ForegroundExposure, color.ForegroundExposure);
        m_Material.SetColor(ShaderProps.Background, color.Background);
        m_Material.SetFloat(ShaderProps.BackgroundExposure, color.BackgroundExposure);

        // update fog settings
        var fog = region.Fog;
        RenderSettings.fogColor = fog.Color;
        RenderSettings.fogEndDistance = fog.EndDistance;
        RenderSettings.fogStartDistance = fog.StartDistance;

        ShaderProps.HeightFog_Color.Set(fog.HeightColor);
        ShaderProps.HeightFog_Density.Set(fog.HeightDensity);
        ShaderProps.HeightFog_MinDist.Set(fog.HeightMin);
    }

    // -- events --
    /// when the region changes
    void OnRegionEntered(Region region) {
        // if this is the first region, set everything directly
        if (m_SrcRegion == null) {
            m_SrcRegion = region;
            m_DstRegion = region;

            m_CurrRegion.Set(region);
            Render();
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