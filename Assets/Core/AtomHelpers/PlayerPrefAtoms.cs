using UnityEngine;
using UnityAtoms;
using UnityAtoms.BaseAtoms;
using System;

public static class PlayerPrefAtoms {
    private const string k_KeyTemplate = "atom_{0}";
    private const string k_ChangedEventTemplate = "{0}_Changed (instanced)";

    /// this is what the method below is trying to do, i'm sorry
    // public static void SetupPlayerPrefs(this FloatVariable variable) {
    //     var key = string.Format(k_KeyTemplate, variable.name);
    //     variable.Value = PlayerPrefs.GetFloat(key, variable.InitialValue);

    //     if(variable.Changed == null) {
    //       var changed = ScriptableObject.CreateInstance<FloatEvent>();
    //       changed.name = string.Format(k_ChangedEventTemplate, variable.name);
    //       variable.Changed = changed;
    //     }

    //     variable.Changed.Register(() => {
    //         PlayerPrefs.SetFloat(key, variable.Value);
    //     });
    // }


    public static AtomEvent<T> SetupPlayerPrefs<T>(AtomBaseVariable<T> variable, AtomEvent<T> changed, Func<string, T> getter, Action<string, T> setter) {
        var key = string.Format(k_KeyTemplate, variable.name);
        variable.Value = getter(key);

        if(changed == null) {
          changed = ScriptableObject.CreateInstance<AtomEvent<T>>();
          changed.name = string.Format(k_ChangedEventTemplate, variable.name);
        }

        changed.Register(() => {
            setter(key, variable.Value);
        });

        return changed;
    }

    public static void SetupPlayerPrefs(this FloatVariable variable) {
        variable.Changed = SetupPlayerPrefs(
            variable, variable.Changed,
            k => PlayerPrefs.GetFloat(k, variable.InitialValue),
            PlayerPrefs.SetFloat) as FloatEvent;
    }

    public static void SetupPlayerPrefs(this StringVariable variable) {
        variable.Changed = SetupPlayerPrefs(
            variable, variable.Changed,
            k => PlayerPrefs.GetString(k, variable.InitialValue),
            PlayerPrefs.SetString) as StringEvent;
    }

    public static void SetupPlayerPrefs(this IntVariable variable) {
        variable.Changed = SetupPlayerPrefs(
            variable, variable.Changed,
            k => PlayerPrefs.GetInt(k, variable.InitialValue),
            PlayerPrefs.SetInt) as IntEvent;
    }

    public static void SetupPlayerPrefs(this BoolVariable variable) {
        variable.Changed = SetupPlayerPrefs(
            variable, variable.Changed,
            k => PlayerPrefs.GetInt(k, variable.InitialValue ? 1 : 0) != 0,
            (k, v) => PlayerPrefs.SetInt(k, v ? 1 : 0)) as BoolEvent;
    }
}