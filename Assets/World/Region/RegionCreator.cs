using UnityEngine;
using UnityAtoms.Discone;

namespace Discone
{
[ExecuteAlways]
public class RegionCreator: MonoBehaviour {
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
        mat.SetFloat(ShaderProps.ForegroundExposure, Region.Value.Sky.ForegroundExposure);
        mat.SetFloat(ShaderProps.BackgroundExposure, Region.Value.Sky.BackgroundExposure);
        mat.SetColor(ShaderProps.Background, Region.Value.Sky.Background);
        mat.SetColor(ShaderProps.Foreground, Region.Value.Sky.Foreground);
    }
}
}