using Cinemachine;
using UnityEngine;

namespace ThirdPerson {

sealed class PlayerCamera: MonoBehaviour {
    // -- refs --
    [Header("refs")]
    [Tooltip("the cinemachine brain")]
    [SerializeField] CinemachineBrain m_Brain;

    // -- props --
    /// the player container
    PlayerContainer c;

    // -- lifecycle --
    void Awake() {
        c = GetComponentInParent<PlayerContainer>();
        // m_Brain.m_CameraActivatedEvent.AddListener(OnCameraActivated);
    }

    // Update is called once per frame
    void OnCameraActivated(ICinemachineCamera dst, ICinemachineCamera src) {
        // if no source camera
        if (src == null) {
            return;
        }

        // if this is another camera
        var cam = c.Camera;
        if (dst.VirtualCameraGameObject != cam.Virtual.VirtualCameraGameObject) {
            return;
        }

        // if switching to the source camera, move camera to position of the
        // previous virtual camera
        // AAA: figure this out
        var pos = m_Brain.OutputCamera.transform.position;
        cam.MoveTo(pos);
    }
}

}