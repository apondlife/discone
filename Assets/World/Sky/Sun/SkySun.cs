using UnityEngine;
using UnityAtoms.BaseAtoms;

/// a sun that changes color
public class SkySun: MonoBehaviour {
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

    /// a set of event subscriptions
    Subscriptions m_Subscriptions = new Subscriptions();

    // -- lifecycle --
    private void Awake() {
        // set props
        m_Renderers = GetComponentsInChildren<Renderer>();

        // bind events
        m_Subscriptions.Add(m_IsHost.Changed, OnIsHostChanged);
    }

    void OnDestroy() {
        // unbind events
        m_Subscriptions.Dispose();
    }

    // -- events --
    /// when the player switches between host/client
    void OnIsHostChanged(bool isHost) {
        // change to the correct color
        var c = isHost ? m_HostColor : m_ClientColor;
        foreach(var r in m_Renderers) {
            r.sharedMaterial.color = c;
        }
    }
}