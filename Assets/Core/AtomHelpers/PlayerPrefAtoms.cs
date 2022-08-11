using UnityEngine;
using UnityAtoms;
using UnityAtoms.BaseAtoms;
using System;

public static class PlayerPrefAtoms {
    private const string k_KeyTemplate = "atom_{0}";
    private const string k_ChangedEventTemplate = "{0}_Changed (instanced)";

    public static void SetupPlayerPrefs(this FloatVariable variable) {
        var key = string.Format(k_KeyTemplate, variable.name);
        variable.Value = PlayerPrefs.GetFloat(key, variable.InitialValue);

        if(variable.Changed == null) {
          var changed = ScriptableObject.CreateInstance<FloatEvent>();
          changed.name = string.Format(k_ChangedEventTemplate, variable.name);
          variable.Changed = changed;
        }

        variable.Changed.Register(() => {
            PlayerPrefs.SetFloat(key, variable.Value);
        });
    }
}