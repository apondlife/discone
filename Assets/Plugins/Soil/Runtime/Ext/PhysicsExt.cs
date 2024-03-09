using UnityEngine;

namespace Soil {

/// static helpers for raycasts
public static class Physiics {
    /// a raycast to and from a point trying to find the last hit
    public static bool RaycastLast(
        Vector3 origin,
        Vector3 destination,
        out RaycastHit hitInfo,
        int layerMask,
        QueryTriggerInteraction queryTriggerInteraction
    ) {
        // to get the last point from origin to destination, we cast from destination to
        var disp = origin - destination;

        return BounceCast(
            destination,
            disp.normalized,
            out hitInfo,
            disp.magnitude,
            layerMask,
            queryTriggerInteraction
        );
    }

    /// a raycast that casts back to its origin from the hit point (or max length, if none)
    public static bool BounceCast(
        Vector3 origin1,
        Vector3 direction,
        out RaycastHit hit,
        float distance,
        int layerMask,
        QueryTriggerInteraction queryTriggerInteraction
    ) {
        // make the first cast in the direction
        var didHit = Physics.Raycast(
            origin1,
            direction,
            out hit,
            distance,
            layerMask,
            queryTriggerInteraction
        );

        // if we hit something, cast back from that point, otherwise, cast from
        // the first cast's max distance
        var origin2 = didHit
            ? hit.point
            : origin1 + direction * distance;

        return Physics.Raycast(
            origin2,
            -direction,
            out hit,
            Vector3.Distance(origin1, origin2),
            layerMask,
            queryTriggerInteraction
        );
    }

    /// a raycast with an origin and line (direction & length)
    public static bool LineCast(
        Vector3 origin,
        Vector3 line,
        out RaycastHit hit,
        int layerMask,
        QueryTriggerInteraction queryTriggerInteraction
    ) {
        return Physics.Raycast(
            origin,
            line.normalized,
            out hit,
            line.magnitude,
            layerMask,
            queryTriggerInteraction
        );
    }
}

}