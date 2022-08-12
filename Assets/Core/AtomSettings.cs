using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityAtoms;
using UnityAtoms.BaseAtoms;

public class AtomSettings : MonoBehaviour
{
    [Tooltip("list of (float/string/int/bool) atoms that will be saved and loaded from player prefs")]
    [SerializeField] List<AtomBaseVariable> m_Settings;

    private void OnValidate() {
        var toRemove = new List<AtomBaseVariable>();
        m_Settings.RemoveAll(setting => {
            switch(setting) {
                case BoolVariable _:
                case FloatVariable _:
                case StringVariable _:
                case IntVariable _:
                    return false;
                default:
                    Debug.LogError($"{setting.name} variable cannot be used in settings");
                    return true;
            }
        });
    }

    void Start()
    {
        foreach(AtomBaseVariable setting in m_Settings) {
            switch(setting) {
                case BoolVariable b:
                    b.SetupPlayerPrefs();
                break;
                case FloatVariable f:
                    f.SetupPlayerPrefs();
                break;
                case StringVariable s:
                    s.SetupPlayerPrefs();
                break;
                case IntVariable i:
                    i.SetupPlayerPrefs();
                break;
                default:
                    Debug.LogError($"{setting.name} variable cannot be used in settings");
                break;
            }
        }
    }
}
