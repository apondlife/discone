Shader "Unlit/UIShader"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _DissolveTex("Dissolve Tex", 2D) = "white" {}
        _DissolveAmount("Dissolve Amount", Range(0,1)) = 0.5
        _DissolveScale("Dissolve Scale", Float) = 1
        _LetterboxAmount("Letterbox Amount", Range(0,1)) = 0.5
        _LetterboxSize("Letterbox Size", Range(0,.5)) = .2
    }
    SubShader
    {
        // tags and stuff taken from
        // https://www.patreon.com/posts/shaders-for-who-29239797
        Tags { 
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        ZTest Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            half _DissolveAmount;
            sampler2D _DissolveTex;
            half _DissolveScale;

            half _LetterboxAmount;
            half _LetterboxSize;
            

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                //fixed4 col = fixed4(1, 0, 0, 0);

                // sample noise tex at this uv
                half4 noise = tex2D(_DissolveTex, i.uv * _DissolveScale);

                half letterboxThreshold = lerp(0, _LetterboxSize, _LetterboxAmount);

                if (i.uv.y < letterboxThreshold || i.uv.y > 1 - letterboxThreshold ) {
                    // letterbox
                    col = fixed4(0, 0, 0, 1);
                } else {
                    // dissolve
                    // noise is b/w, so we just take red
                    clip(noise.r - _DissolveAmount);
                }

              

  

                // if (false) {
                //     discard;
                // }

                // if (col.a == 0) {
                //     col.r = 0;
                // }

                // fixed alpha = step(noise.r, _DissolveAmount);
                // col.a = alpha;
                // if ()
                //col.r = 1;

                //clip (col.a - .001);

                

                return col;
            }
            ENDCG
        }
    }
}
