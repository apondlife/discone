using UnityEngine;

/// extensions for layers and layer masks
static class LayerExt {
    /// create a layer mask from the names
    public static LayerMask MaskFromNames(params string[] names) {
        var mask = 0;
        foreach (var name in names) {
            mask |= 1 << LayerMask.NameToLayer(name);
        }
        return mask;
    }
}