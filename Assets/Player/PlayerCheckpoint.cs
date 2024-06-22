using UnityEngine;
using UnityAtoms;
using UnityEngine.InputSystem;

namespace Discone {

/// the discone checkpoint controller
public sealed class PlayerCheckpoint: MonoBehaviour {
    // -- atoms --
    [Header("atoms")]
    [Tooltip("the progress of the checkpoint save")]
    [SerializeField] CharacterVariable m_Character;

    // -- props --
    /// if the checkpoint was saving previously
    bool m_PrevIsSaving;

    /// if the saving value changed
    bool m_IsSavingChanged;

    // -- lifecycle --
    void Update() {
        // coordinate input & current character's checkpoint
        var character = m_Character.Value;
        if (!character || !character.Checkpoint) {
            return;
        }

        var isSaving = IsSaving;
        m_IsSavingChanged = m_PrevIsSaving != isSaving;
        m_PrevIsSaving = isSaving;
    }

    // -- queries --
    /// if currently saving a checkpoint
    public bool IsSaving {
        get => m_Character?.Value?.Checkpoint.IsSaving ?? false;
    }

    /// if the saving value changed
    public bool IsSavingChanged {
        get => m_IsSavingChanged;
    }
}

}