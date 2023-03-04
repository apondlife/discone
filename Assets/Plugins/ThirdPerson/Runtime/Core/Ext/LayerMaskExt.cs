using UnityEngine;

namespace ThirdPerson {

public static class LayerMaskExt {
    public static bool Contains(this LayerMask mask, int layer) {
        return (mask & (1 << layer)) != 0;
    }
}

}