using System;
using Mirror;
using Soil;
using ThirdPerson;
using UnityEngine;
using UnityEngine.Serialization;

namespace Discone {

/// the index of the color in the palette
[Flags]
enum PaletteIndex {
    Color0 = 1 << 0,
    Color1 = 1 << 1,
    Color2 = 1 << 2,
    Color3 = 1 << 3,
    Color4 = 1 << 4,
    Color5 = 1 << 5,
    Color6 = 1 << 6,
}

// -- state --
public sealed partial class Character_Online {
    // -- refs --
    [FormerlySerializedAs("m_Presentation")]
    [Header("palette")]
    [Tooltip("changes the color of the character")]
    [SerializeField] CharacterPalette m_Palette;

    // -- props --
    /// the current hue shift
    [SyncVar(hook = nameof(OnShiftChanged))]
    float m_Shift;

    /// the previous hue shift
    float m_PrevShift;

    // -- commands --
    [Server]
    public void ShiftHue(float shift) {
        m_Shift = shift;
    }

    // -- queries --
    public float NextShift {
        get => m_Shift - m_PrevShift;
    }

    // -- events --
    /// shift the hue of the configured colors
    void OnShiftChanged(float prevShift, float nextShift) {
        m_PrevShift = prevShift;
        m_Palette.ShiftToCurrentHue();
    }
}

// -- presentation --
/// the character's color palette
public sealed class CharacterPalette: MonoBehaviour {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the set of colors for this character that hue shift")]
    [SerializeField] PaletteIndex m_ShiftedColors;

    // -- props --
    // TODO: should we create a child interface for discone-specific CharacterContainer props?
    /// .
    Character c;

    // -- lifecycle --
    void Start() {
        // set dependencies
        c = GetComponentInParent<Character>();

        // shift to the initial hue if set previously
        ShiftToCurrentHue();
    }

    // -- commands --
    /// shift to the current hue
    public void ShiftToCurrentHue() {
        if (!c || !c.Model || c.Model.Materials == null) {
            return;
        }

        foreach (var material in c.Model.Materials.All) {
            for (var i = 0; i < 7; i++) {
                var index = 1 << i;
                if (!m_ShiftedColors.HasFlag((PaletteIndex)index)) {
                    continue;
                }

                var prop = ShaderProps.Colors[i];
                if (!material.HasColor(prop)) {
                    continue;
                }

                var prevColor = material.GetColor(prop);
                var nextColor = prevColor.RotateHue(c.Online.NextShift);
                material.SetColor(prop, nextColor);
            }
        }
    }
}

}