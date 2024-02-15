using UnityAtoms;
using UnityEngine;

namespace Discone {

sealed class Dream: MonoBehaviour {
    [Header("refs")]
    [SerializeField] Transform m_FlowerPosition;
    [SerializeField] DisconeCharacterVariable m_CurrentCharacter;

    [Tooltip("the shared data store")]
    [SerializeField] Store m_Store;

    /// -- props --
    /// the set of event subscriptions
    DisposeBag m_Subscriptions = new DisposeBag();

    /// -- lifecycle --
    void Start() {

        m_Subscriptions
            .Add(m_Store.LoadFinished, OnLoadFinished)
            .Add(m_CurrentCharacter.ChangedWithHistory, OnCurrentCharacterChanged);
    }

    /// -- events --
    void OnLoadFinished() {
        Debug.Log($"[dream] OnLoadFinished {m_Store.Player.HasData}");
    }

    void OnCurrentCharacterChanged(DisconeCharacterPair _) {
        Debug.Log("[dream] OnCharacterChanged");
        if(m_Store.Player.HasData) {
            DestroyImmediate(this);
            return;
        }

        m_CurrentCharacter.Value.PlantFlower(Checkpoint.FromTransform(m_FlowerPosition));
    }
}

}