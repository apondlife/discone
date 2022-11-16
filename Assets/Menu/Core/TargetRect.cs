using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Discone.Ui {

/// a target that emits rect changes
[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
sealed class TargetRect: UIBehaviour {
    // -- props --
    /// when the rect dimensions change
    public Action Changed;

    // -- lifecyle --
    override protected void OnRectTransformDimensionsChange() {
        base.OnRectTransformDimensionsChange();

        // fire the event
        Changed?.Invoke();
    }

    override protected void OnDestroy() {
        // clear events
        Changed = null;
    }
}

}