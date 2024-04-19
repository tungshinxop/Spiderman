Shader "iGame/iKameNormalMap"
{
    Properties
    {
        [Enum(UnityEngine.Rendering.CullMode)] _Culling("Culling", Float) = 0

        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _BumpMap ("Normal Map", 2D) = "bump" {}

        _NormalStrength ("Normal Strength", Range(-10,10)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard

        sampler2D _MainTex;
        sampler2D _BumpMap;

        struct Input
        {
            float2 uv_MainTex;
        };

        fixed4 _Color;
        half _NormalStrength;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;

            o.Albedo = c.rgb;

            fixed3 bump = UnpackNormal(tex2D (_BumpMap, IN.uv_MainTex));
            bump = fixed3(bump.r * _NormalStrength, bump.g * _NormalStrength, bump.b);
            o.Normal = bump;

            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
