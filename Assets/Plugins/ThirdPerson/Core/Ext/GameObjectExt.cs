using UnityEngine;

namespace ThirdPerson {

public static class GameObjectExt {
    public static void SetLayerRecursively(this GameObject obj, int layer) {
        obj.layer = layer;

        foreach(Transform child in obj.transform) {
            child.gameObject.SetLayerRecursively(layer);
        }
    }
}

}