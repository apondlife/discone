using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityAtoms.Discone;

[ExecuteAlways]
public class RegionCreator : MonoBehaviour
{
    [SerializeField]
    private TMPro.TMP_Text text;
    // private Region region;

    public RegionConstant Region;

    // Update is called once per frame
    void Update()
    {
        if(Region == null) return;
        text.text = Region.Value.DisplayName;
        var mat = RenderSettings.skybox;
        mat.SetFloat("_ExposureForeground", Region.Value.SkyboxExposureForeground);
        mat.SetFloat("_ExposureBackground", Region.Value.SkyboxExposureBackground);
        mat.SetColor("_Background", Region.Value.SkyboxColorBackground);
        mat.SetColor("_Foreground", Region.Value.SkyboxColorForeground);

    }
}
