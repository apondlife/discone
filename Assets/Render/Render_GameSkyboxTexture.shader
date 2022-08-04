Shader "Custom/Render/GameSkyboxTexture" {

Properties {
    _MainTex ("Texture", 2D) = "white" {}
}

SubShader {
    Tags {
        "Queue"       = "Background"
        "RenderType"  = "Background"
        "PreviewType" = "Skybox"
    }

    Cull Off
    ZWrite Off

    Pass {
        CGPROGRAM

        // -- config --
        #pragma vertex DrawVert
        #pragma fragment DrawFrag

        // -- includes --
        #include "UnityCG.cginc"
        #include "Assets/Shaders/Core/Math.cginc"

        // -- props --
        /// the texture
        sampler2D _MainTex;

        // -- types --
        /// the vertex shader input
        struct VertIn {
            float4 pos : POSITION;
            float4 uv : TEXCOORD0;
        };

        /// the fragment shader input
        struct FragIn {
            float4 pos : SV_POSITION;
            float4 uv : TEXCOORD0;
        };

        // -- program --
        /// the vertex shader
        FragIn DrawVert(VertIn i) {
            FragIn o;
            o.pos = UnityObjectToClipPos(i.pos);
            o.uv = ComputeScreenPos(o.pos);
            return o;
        }

        /// the fragment shader
        fixed4 DrawFrag(FragIn i): SV_Target {
            return tex2Dproj(_MainTex, UNITY_PROJ_COORD(i.uv));
        }
        ENDCG
    }
}

Fallback Off

}