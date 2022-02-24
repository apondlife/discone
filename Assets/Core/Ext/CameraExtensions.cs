using UnityEngine;

public static class CameraExtensions
{
  // http://wiki.unity3d.com/index.php/IsVisibleFrom
  public static bool IsVisibleFromCamera(this Camera camera, Renderer renderer)
  {
    Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
    return GeometryUtility.TestPlanesAABB(frustumPlanes, renderer.bounds);
  }
}