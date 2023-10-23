Shader "Hai/HaiCgeGrayscale"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _Desaturate ("Desaturate", Range(0, 1)) = 1
        _Converge ("Graying", Range(0, 1)) = 0.2
        _Brightness ("Brightness", Range(0, 1)) = 0.1
        _OverlayTex ("Overlay Texture", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
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
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float _Desaturate;
            float _Converge;
            float _Brightness;

            sampler2D _OverlayTex;
            float4 _OverlayTex_ST;
            float4 _OverlayTex_TexelSize;

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
                fixed avg = (col.r + col.g + col.b) / 3.0;

                fixed4 desaturate = lerp(col, fixed4(avg, avg, avg, col.a), _Desaturate);
                fixed4 converged = lerp(desaturate, fixed4(_Brightness, _Brightness, _Brightness, col.a), _Converge);

                fixed4 over = tex2D(_OverlayTex, i.uv);
                fixed4 result = lerp(converged, over, over.a);
                result.a = saturate(converged.a + over.a);

                return result;
            }
            ENDCG
        }
    }
}
