using UnityEngine;

/// extensions to unity materials
static class MaterialExt {
    /// creates an unsaved copy of this material in editor
    public static Material Unsaved(this Material mat) {
        #if UNITY_EDITOR
            return new Material(mat);
        #else
            return mat;
        #endif
    }
}
