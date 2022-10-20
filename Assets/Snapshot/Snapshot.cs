using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Discone.Tools {

public class Snapshot : MonoBehaviour {

    [SerializeField] bool m_SnapshotOnStart = false;

    [Header("config path")]
    [Tooltip("$GAME= productName, $TITLE=specifiedbelow, $DATE = current time formatted as specified below")]
    [SerializeField] string m_NameTemplate = "$GAME_$TITLE_$DATE";
    [SerializeField] string m_DateTimeFormat = "yyyy-MM-dd_HH-mm-ss";
    [SerializeField] string m_Path = "../Artifacts/Snapshots";
    [SerializeField] string m_Title;

    [Header("def")]
    [SerializeField] int m_Width = 1920;
    [SerializeField] int m_Height = 1080;

    [Header("refs")]
    [SerializeField] Camera m_Camera;
    [SerializeField] RenderTexture m_RenderTexture;

    // -- lifecycle
    void OnValidate()
    {
        if (m_Camera == null) {
            m_Camera = GetComponent<Camera>();
        }
        if (m_Camera == null) {
            m_Camera = GetComponentInChildren<Camera>();
        }

        if(string.IsNullOrEmpty(m_Title) && m_Camera != null) {
            m_Title = m_Camera.gameObject.name;
        }
    }

    public void Start() {
        if (m_SnapshotOnStart) {
            this.DoAfterTime(1.0f, TakeSnapshot);
        }
    }

    public void TakeSnapshot() {
        if (m_Camera == null) {
            Debug.LogError("[snapshot] There is no camera set for the snapshot");
            return;
        }

        RenderTexture activeRenderTexture = RenderTexture.active;
        var renderTexture = GetRenderTexture();

        RenderTexture.active = renderTexture;
        var camTexture = m_Camera.targetTexture;
        m_Camera.targetTexture = renderTexture;

        m_Camera.Render();

        Texture2D image = new Texture2D(renderTexture.width, renderTexture.height);
        image.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        image.Apply();

        RenderTexture.active = activeRenderTexture;
        m_Camera.targetTexture = camTexture;

        byte[] bytes = image.EncodeToPNG();

        Destroy(image);

        var fileName = m_NameTemplate
            .Replace("$GAME", Application.productName)
            .Replace("$TITLE", m_Title)
            .Replace("$DATE", System.DateTime.Now.ToString(m_DateTimeFormat)) + ".png";

        var location = $"{Application.dataPath}//{m_Path}//{fileName}";

        Directory.CreateDirectory(Path.GetDirectoryName(location));
        File.WriteAllBytes(location, bytes);

        Debug.Log($"[snapshot] new image created at: {location}");
    }

    // -- queries --
    private RenderTexture GetRenderTexture() {
        if (m_RenderTexture != null) {
            return m_RenderTexture;
        }

        if (m_Camera?.targetTexture != null) {
            m_RenderTexture = m_Camera.targetTexture;
            Debug.Log("[snapshot] snapshot is overriding render texture settings to use the camera's texture");
        }
        else {
            m_RenderTexture = new RenderTexture(m_Width, m_Height, 16, RenderTextureFormat.Default);
            m_RenderTexture.Create();
        }

        return m_RenderTexture;
    }
}
}


