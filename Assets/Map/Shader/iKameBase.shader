Shader "iGame/iKameBase"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque"
        }
        LOD 200

        CGPROGRAM
        #pragma multi_compile _ LOD_FADE_CROSSFADE
        #pragma surface surf Standard fullforwardshadows

        sampler2D _MainTex;

        struct Input
        {
            float4 screenPos;
            float2 uv_MainTex;
        };

        fixed4 _Color;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
        //#ifdef LOD_FADE_CROSSFADE
        //    float2 vpos = IN.screenPos.xy / IN.screenPos.w * _ScreenParams.xy;
        //    UnityApplyDitherCrossFade(vpos);
        //#endif
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
