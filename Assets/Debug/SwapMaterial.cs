#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// swap materials for all children between two channels
public class SwapMaterial: MonoBehaviour {
    // -- input --
    [Header("input")]
    [Tooltip("the name of the material palette")]
    [SerializeField] string m_Palette;

    // -- config --
    [Header("config")]
    [Tooltip("the target object")]
    [SerializeField] GameObject m_Target;

    // -- defaults --
    [Header("defaults")]
    [Tooltip("the default main material")]
    [SerializeField] Material m_Main;

    [Tooltip("the default human material")]
    [SerializeField] Material m_Human;

    [Tooltip("the default bowl material")]
    [SerializeField] Material m_Bowl;

    // -- commands --
    [ContextMenu("Load Materials")]
    void LoadMaterials() {
        // find the matching assets
        var guids = null as string[];
        if (m_Palette != "") {
            guids = AssetDatabase.FindAssets($"t:material Incline_{m_Palette}_");
        }

        // find the set of the materials
        var materials = DefaultMaterials;
        if (guids != null && guids.Length == 3) {
            // find the set of the materials
            foreach (var guid in guids) {
                var path = AssetDatabase.GUIDToAssetPath(guid);

                var key = FindKeyFromName(path);
                if (key == null) {
                    Debug.LogError($"[swap materials] cannot find key from {name}");
                    continue;
                }

                materials[key] = AssetDatabase.LoadAssetAtPath<Material>(path);;
            }
        }

        // swap all materials
        foreach (var renderer in m_Target.GetComponentsInChildren<Renderer>()) {
            var key = FindKeyFromName(renderer.sharedMaterial.name);
            if (key == null) {
                continue;
            }

            renderer.sharedMaterial = materials[key];
        }
    }

    // -- queries --
    /// a memoized dictionray of default materials
    Dictionary<string, Material> DefaultMaterials {
        get {
            var defaults = new Dictionary<string, Material>();
            defaults["Main"] = m_Main;
            defaults["Human"] = m_Human;
            defaults["Bowl"] = m_Bowl;
            return defaults;
        }
    }

    /// extract material key from path/name
    string FindKeyFromName(string name) {
        return name switch {
            var s when s.Contains("Main") => "Main",
            var s when s.Contains("Bowl") => "Bowl",
            var s when s.Contains("Human") => "Human",
            _ => null
        };
    }
}
#endif