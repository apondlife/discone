#if UNITY_EDITOR
using NaughtyAttributes;
using UnityEngine;
using UnityAtoms.Discone;
using UnityEditor;

namespace Discone {

public partial class Atmosphere {
    // -- editing --
    [Header("editing")]
    [Tooltip("the transition duration between region colors")]
    [SerializeField] RegionConstant m_Editing;

    [Label("Editing?")]
    [EnableIf(nameof(HasEdits))]
    [Tooltip("if we are showing the editing region")]
    [SerializeField] bool m_IsEditing;

    // -- props --
    /// a temporary region to swap during editing
    Region m_TempRegion;

    /// if we were just editing
    bool m_WasEditing;

    // -- lifecycle --
    void LateUpdate() {
        // lazily initialize a temp region
        if (m_TempRegion == null) {
            m_TempRegion = m_CurrRegion.Copy();
        }

        // if changed, show the temp or editing region
        if (m_IsEditing != m_WasEditing) {
            if (m_IsEditing) {
                m_CurrRegion.Name = "Temp";
                m_CurrRegion.Set(m_TempRegion);
            } else if (m_Editing.Value != null) {
                var region = m_Editing.Value;
                m_CurrRegion.Name = region.Name;
                m_CurrRegion.Set(region);
            }
        }

        // update temp while editing
        if (m_IsEditing) {
            m_TempRegion.Set(m_CurrRegion);
        }

        // sync flag
        m_WasEditing = m_IsEditing;

        // draw the current sky
        Render();
    }

    // -- commands --
    /// save the current settings as a region
    [Button("Save")]
    [EnableIf(nameof(m_IsEditing))]
    void Save() {
        // edit the asset
        var asset = m_Editing;

        // or create one if not editing
        if (asset == null) {
            asset = ScriptableObject.CreateInstance<RegionConstant>();
            AssetDatabase.CreateAsset(asset, "Assets/World/Editor/TempRegion.asset");
        }

        // update the region
        asset.Value.Set(m_CurrRegion);

        // and save the asset
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // apply future edits to it if necessary
        m_Editing = asset;
    }

    /// reset temp to the editing region
    [Button("Reset Temp")]
    [EnableIf(nameof(HasEdits))]
    void ResetTemp() {
        m_TempRegion.Set(m_Editing.Value);
    }

    // -- queries --
    /// if we are editing a region asset
    bool HasEdits {
        get => m_Editing != null;
    }
}

}
#endif