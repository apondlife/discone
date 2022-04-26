using UnityEngine;
using UnityAtoms.BaseAtoms;

/// the root audio script
public class Audio: MonoBehaviour {
    // -- constants --
    /// the maximum volume in decibels
    const float k_MaxVolumeScale = 2.0f;

    /// the name of the main bus
    const string k_MainBusName = "bus:/";

    /// the name of the music bus
    const string k_MusicBusName = "bus:/Music";

    /// the name of the effects bus
    const string k_SfxBusName = "bus:/Effects";

    // -- fields --
    [Header("fields")]
    [Tooltip("the main volume")]
    [SerializeField] FloatVariable m_MainVolume;

    [Tooltip("the music volume")]
    [SerializeField] FloatVariable m_MusicVolume;

    [Tooltip("the effects volume")]
    [SerializeField] FloatVariable m_SfxVolume;

    // -- props --
    /// the main bus
    FMOD.Studio.Bus m_MainBus;

    /// the music bus
    FMOD.Studio.Bus m_MusicBus;

    /// the effects bus
    FMOD.Studio.Bus m_SfxBus;

    /// the subscriptions
    Subscriptions m_Subscriptions = new Subscriptions();

    // -- lifecycle --
    void Start() {
        // set props
        m_MainBus = FMODUnity.RuntimeManager.GetBus(k_MainBusName);
        m_MusicBus = FMODUnity.RuntimeManager.GetBus(k_MusicBusName);
        m_SfxBus = FMODUnity.RuntimeManager.GetBus(k_SfxBusName);

        // bind events
        m_Subscriptions
            .Add(m_MainVolume.Changed, OnMainVolumeChanged)
            .Add(m_MusicVolume.Changed, OnMusicVolumeChanged)
            .Add(m_SfxVolume.Changed, OnSfxVolumeChanged);
    }

    void OnDestroy() {
        // unbind events
        m_Subscriptions.Dispose();
    }

    // -- commands --
    /// set the volume of a particular bus; volume is [0,1]
    void SetBusVolume(FMOD.Studio.Bus bus, float pct) {
        // the volume is a scale on top of something constant set in fmod studio
        var scale = Mathf.Lerp(0.0f, k_MaxVolumeScale, pct);
        Debug.Log($"set pct {pct} vol {scale}");
        bus.setVolume(scale);
    }

    // -- events --
    void OnMainVolumeChanged(float volume) {
        SetBusVolume(m_MainBus, volume);
    }

    void OnMusicVolumeChanged(float volume) {
        SetBusVolume(m_MusicBus, volume);
    }

    void OnSfxVolumeChanged(float volume) {
        SetBusVolume(m_SfxBus, volume);
    }
}
