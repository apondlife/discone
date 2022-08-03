using UnityEngine;
using UnityAtoms.BaseAtoms;

/// a sun that changes color
[RequireComponent(typeof(SkyChartBody))]
public class Sun: MonoBehaviour {
  // -- config --
  [Header("config")]
  [Tooltip("the sun color when hosting a server")]
  [ColorUsage(true, true)]
  [SerializeField] Color m_HostColor;

  [Tooltip("the sun color when connected as a client")]
  [ColorUsage(true, true)]
  [SerializeField] Color m_ClientColor;

  // -- refs --
  [Header("refs")]
  [Tooltip("if the player is the host")]
  [SerializeField] BoolVariable m_IsHost;

  // -- props --
  /// the list of renderers
  Renderer[] m_Renderers;

  // -- lifecycle --
  private void Awake() {
    m_Renderers = GetComponentsInChildren<Renderer>();
    m_IsHost.Changed.Register(isHost => {
      SetColor(isHost ? m_HostColor : m_ClientColor);
    });
  }

  // -- commands --
  private void SetColor(Color c) {
    foreach(var r in m_Renderers) {
      r.sharedMaterial.color = c;
    }
  }
}