using UnityEditor.Overlays;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

using E = UnityEditor.EditorGUILayout;
using G = UnityEngine.GUILayout;
using UnityAtoms;

namespace Discone.Editor {

[Overlay(typeof(SceneView), k_Id, "discone")]
class DisconeOverlay : IMGUIOverlay
{
    // -- constants --
    const string k_Id = "discone-overlay";

    const string k_EntitiesPath = "Assets/Entity/Entities.asset";

    // -- fields --
    /// the entity repos
    EntitiesVariable m_Entities;

    public override void OnGUI() {
        ToggleTerain();
        SelectCharacter();
    }

    void ToggleTerain() {
        var value = WorldChunks.IsCreatingTerrain;
        var label = value ? "disable terrain" : "enable terrain";

        if (G.Button(label)) {
                WorldChunks.ToggleIsCreatingTerrain();
        }
    }

    void SelectCharacter() {
        if (m_Entities == null) {
            m_Entities = AssetDatabase.LoadAssetAtPath<EntitiesVariable>(k_EntitiesPath);
        }

        var entities = m_Entities.Value;

        if (entities == null) {
            return;
        }

        // find current character
        var character = entities?
            .OnlinePlayers
            .Current?
            .Character;

        if (G.Button("select character") && character != null) {
            // select their character
            Selection.activeGameObject = character.transform.gameObject;
        }
    }
}

}
