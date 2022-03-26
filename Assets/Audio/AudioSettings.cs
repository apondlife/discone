using UnityEngine;
using UnityEngine.Audio;
using UnityAtoms.BaseAtoms;

public class AudioSettings : MonoBehaviour
{
    public FloatVariable MasterVolume;
    public FloatVariable SfxVolume;
    public FloatVariable MusicVolume;
    public AudioMixer mixer;

    private Subscriptions subscriptions;

    // Start is called before the first frame update
    void Start()
    {
        mixer.GetFloat("volumeMaster", out var masterVolume);
        MasterVolume.Value = masterVolume;
        subscriptions.Add(MasterVolume.Changed, (v) =>
        {
            v = Mathf.Max(v, 0.00001f);
            mixer.SetFloat("volumeMaster", Mathf.Log10(v) * 20);
        });

        mixer.GetFloat("volumeMusic", out var musicVolume);
        MusicVolume.Value = musicVolume;
        subscriptions.Add(MusicVolume.Changed, (v) =>
        {
            v = Mathf.Max(v, 0.00001f);
            mixer.SetFloat("volumeMusic", Mathf.Log10(v) * 20);
        });

        mixer.GetFloat("volumeSfx", out var sfxVolume);
        SfxVolume.Value = sfxVolume;
        subscriptions.Add(SfxVolume.Changed, (v) =>
        {
            v = Mathf.Max(v, 0.00001f);
            mixer.SetFloat("volumeSfx", Mathf.Log10(v) * 20);
        });
    }

    private void OnDestroy() {
        subscriptions.Dispose();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            MasterVolume.Value -= 1;
        }
        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            MasterVolume.Value += 1;
        }
    }
}
