using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// a target that emits rect changes
[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class TargetRect: UIBehaviour {
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