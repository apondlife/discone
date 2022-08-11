using FMODUnity;
using UnityEngine;
using UnityAtoms;
using UnityAtoms.BaseAtoms;

/// the root audio script
public class Audio: MonoBehaviour {
    // -- constants --
    /// the maximum volume in decibels
    const float k_MaxVolumeScale = 0.63f;

    /// the name of the main bus
    const string k_MainBusName = "bus:/";

    /// the name of the music bus
    const string k_MusicBusName = "bus:/Music";

    /// the name of the effects bus
    const string k_SfxBusName = "bus:/Effects";

    // -- state --
    [Header("state")]
    [Tooltip("the main volume")]
    [SerializeField] FloatVariable m_MainVolume;

    [Tooltip("the music volume")]
    [SerializeField] FloatVariable m_MusicVolume;

    [Tooltip("the effects volume")]
    [SerializeField] FloatVariable m_SfxVolume;

    // -- refs --
    [Header("refs")]
    [Tooltip("the music emitter")]
    [SerializeField] StudioEventEmitter m_Music;

    [Tooltip("when the local character changes")]
    [SerializeField] DisconeCharacterPairEvent m_CharacterChangedWithHistory;

    // -- props --
    /// the main bus
    FMOD.Studio.Bus m_MainBus;

    /// the music bus
    FMOD.Studio.Bus m_MusicBus;

    /// the effects bus
    FMOD.Studio.Bus m_SfxBus;

    /// if this music is playing
    bool m_IsMusicPlaying = false;

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
            .Add(m_SfxVolume.Changed, OnSfxVolumeChanged)
            .Add(m_CharacterChangedWithHistory, OnCharacterChanged);


        // set it up from player prefs
        m_MainVolume.SetupPlayerPrefs();
        m_MusicVolume.SetupPlayerPrefs();
        m_SfxVolume.SetupPlayerPrefs();
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
        bus.setVolume(scale);
    }

    // -- events --
    /// when the main volume changes
    void OnMainVolumeChanged(float volume) {
        SetBusVolume(m_MainBus, volume);
    }

    /// when the music volume changes
    void OnMusicVolumeChanged(float volume) {
        SetBusVolume(m_MusicBus, volume);
    }

    /// when the sound effects volume changes
    void OnSfxVolumeChanged(float volume) {
        SetBusVolume(m_SfxBus, volume);
    }

    /// when the local character changes
    void OnCharacterChanged(DisconeCharacterPair change) {
        var curr = change.Item1;

        // the first time the player changes to a character
        if (!m_IsMusicPlaying && curr != null) {
            m_IsMusicPlaying = true;
            m_Music.Play();
        }
    }
}
