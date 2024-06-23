using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ThirdPerson {

/// the collection of materials in the model
public class CharacterMaterials {
    // -- props --
    /// the list of all materials
    Material[] m_Materials;

    // -- lifecycle --
    public CharacterMaterials(CharacterModel model) {
        // aggregate a list of materials
        var materials = new HashSet<Material>();
        var renderers = model.GetComponentsInChildren<Renderer>(true);

        foreach (var r in renderers) {
            materials.UnionWith(r.materials);
        }

        m_Materials = materials.ToArray();
    }

    // -- queries --
    /// the list of all materials
    public IEnumerable<Material> All {
        get => m_Materials;
    }
}

}