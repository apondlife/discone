#if UNITY_EDITOR

namespace Discone {

static class EditorCamera {
    /// get the editor camera, if this is a valid context to do so
    public static UnityEngine.Camera Get {
        get {
            // get the editor camera
            var scene = UnityEditor.SceneView.lastActiveSceneView;
            if (scene == null) {
                return null;
            }

            var camera = scene.camera;
            if (camera == null) {
                return null;
            }

            // don't create chunks in prefab mode
            var preview = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            if (preview != null) {
                return null;
            }

            // produce a camera, finally
            return camera;
        }
    }
}

}

#endif