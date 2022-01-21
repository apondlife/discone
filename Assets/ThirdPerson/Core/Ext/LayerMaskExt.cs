using UnityEngine;

public static class LayerMaskExt {
    public static bool Contains(this LayerMask layerMask, int layer)
        => layerMask == (layerMask | (1 << layer));
}