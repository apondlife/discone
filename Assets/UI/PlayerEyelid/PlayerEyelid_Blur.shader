// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Discone/PlayerEyelid_Blur" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "" {}
    }

    CGINCLUDE
    // -- includes --
    #include "UnityCG.cginc"

    // -- types --
    struct FragIn {
        float4 pos : POSITION;
        float2 uv : TEXCOORD0;
        float4 uv01 : TEXCOORD1;
        float4 uv23 : TEXCOORD2;
        float4 uv45 : TEXCOORD3;
    };

    // -- props --
    // the input texture
    sampler2D _MainTex;

    // the separable blur offsets
    float4 _Offsets;

    // -- program --
    FragIn DrawVert (appdata_img v) {
        FragIn o;
        o.pos = UnityObjectToClipPos(v.vertex);
        o.uv.xy = v.texcoord.xy;

        o.uv01 =  v.texcoord.xyxy + _Offsets.xyxy * float4(1, 1, -1, -1);
        o.uv23 =  v.texcoord.xyxy + _Offsets.xyxy * float4(1, 1, -1, -1) * 2.0;
        o.uv45 =  v.texcoord.xyxy + _Offsets.xyxy * float4(1, 1, -1, -1) * 3.0;

        return o;
    }

    fixed4 DrawFrag (FragIn i) : COLOR {
        fixed4 color = fixed4(0, 0, 0, 0);

        color += 0.40 * tex2D(_MainTex, i.uv);
        color += 0.15 * tex2D(_MainTex, i.uv01.xy);
        color += 0.15 * tex2D(_MainTex, i.uv01.zw);
        color += 0.10 * tex2D(_MainTex, i.uv23.xy);
        color += 0.10 * tex2D(_MainTex, i.uv23.zw);
        color += 0.05 * tex2D(_MainTex, i.uv45.xy);
        color += 0.05 * tex2D(_MainTex, i.uv45.zw);

        return color;
    }
    ENDCG

    Subshader {
        Pass {
            ZTest Always
            Cull Off
            ZWrite Off
            Fog { Mode off }

            CGPROGRAM
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma vertex DrawVert
            #pragma fragment DrawFrag
            ENDCG
        }
    }

    Fallback off
}
