using UnityEngine;
using UnityEngine.Audio;
using UnityAtoms.BaseAtoms;

public class AudioSettings : MonoBehaviour
{
    public FloatVariable MasterVolume;
    public FloatVariable SfxVolume;
    public FloatVariable MusicVolume;
    public AudioMixer mixer;

    // Start is called before the first frame update
    void Start()
    {
        mixer.GetFloat("volumeMaster", out var masterVolume);
        MasterVolume.Value = masterVolume;
        MasterVolume.Changed.Register((v) =>
        {
            v = Mathf.Max(v, 0.00001f);
            mixer.SetFloat("volumeMaster", Mathf.Log10(v) * 20);
        });

        mixer.GetFloat("volumeMusic", out var musicVolume);
        MusicVolume.Value = musicVolume;
        MusicVolume.Changed.Register((v) =>
        {
            v = Mathf.Max(v, 0.00001f);
            mixer.SetFloat("volumeMusic", Mathf.Log10(v) * 20);
        });

        mixer.GetFloat("volumeSfx", out var sfxVolume);
        SfxVolume.Value = sfxVolume;
        SfxVolume.Changed.Register((v) =>
        {
            v = Mathf.Max(v, 0.00001f);
            mixer.SetFloat("volumeSfx", Mathf.Log10(v) * 20);
        });
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
