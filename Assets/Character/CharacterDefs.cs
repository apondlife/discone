using UnityEngine;

namespace Discone {

[CreateAssetMenu(menuName = "Character/Defs", fileName = "Characters")]
sealed class CharacterDefs: ScriptableObject {
    // -- module --
    /// storage for the singleton
    static CharacterDefs s_Instance;

    /// the singleton
    public static CharacterDefs Instance {
        get => s_Instance;
    }

    // -- fields --
    [Header("fields")]
    [Tooltip("the icecream definition")]
    [SerializeField] CharacterDef m_Icecream;

    [Tooltip("the ivan definition")]
    [SerializeField] CharacterDef m_Ivan;

    [Tooltip("the frog definition")]
    [SerializeField] CharacterDef m_Frog;

    [Tooltip("the frog definition")]
    [SerializeField] CharacterDef m_Clockboi;

    // -- lifecycle --
    void OnEnable() {
        Log.Charss.Assert(s_Instance == null, "already an instance of `CharacterDefs`");
        s_Instance = this;
    }

    // -- queries --
    /// find a character def by name
    public CharacterDef Find(CharacterKey key) {
        var def = (key) switch {
            CharacterKey.Icecream => m_Icecream,
            CharacterKey.Ivan => m_Ivan,
            CharacterKey.Frog => m_Frog,
            CharacterKey.Clockboi => m_Clockboi,
            _ => null,
        };

        if (def == null) {
            Log.Charss.E($"no definition found for character {key}");
        }

        return def;
    }
}

}