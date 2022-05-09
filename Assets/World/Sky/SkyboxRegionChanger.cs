using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityAtoms.Discone;

public class SkyboxRegionChanger : MonoBehaviour
{
    [Header("tunables")]
    [SerializeField]
    private float m_ChangeDuration;


    [Header("References")]
    [SerializeField]
    private Material m_SkyboxMaterial;
    [Header("Atoms")]
    [SerializeField]
    private RegionEvent m_RegionEntered;

    Subscriptions m_Subscriptions = new Subscriptions();

    void Start()
    {
        m_Subscriptions.Add(m_RegionEntered, OnRegionEntered);

    }

    void OnDestroy() {
        // unbind events
        m_Subscriptions.Dispose();
    }

    void OnRegionEntered(Region region)
    {
        var background = m_SkyboxMaterial.GetColor("_Background");
        var foreground = m_SkyboxMaterial.GetColor("_Foreground");
        var exposure = m_SkyboxMaterial.GetFloat("_Exposure");
        StartCoroutine(CoroutineHelpers.InterpolateByTime(m_ChangeDuration, k => {
            m_SkyboxMaterial.SetColor("_Background", Color.Lerp(background, region.SkyboxColorBackground, k));
            m_SkyboxMaterial.SetColor("_Foreground", Color.Lerp(foreground, region.SkyboxColorForeground, k));
            m_SkyboxMaterial.SetFloat("_Exposure", Mathf.Lerp(exposure, region.SkyboxExposure, k));
        }));
    }
}
