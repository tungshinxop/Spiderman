Shader "iKame/iKameFres"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Fres ("Out", Range(0,10)) = 0.5
		_Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200


        CGPROGRAM
        #pragma surface surf Lambert addshadow

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
			float3 viewDir;
        };

        half _Fres;
        fixed4 _Color;

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb;
			half rim = 1.0 - saturate(dot (normalize(IN.viewDir), o.Normal));
			o.Emission = (_Color * pow (rim, _Fres));
        }
        ENDCG
    }
    FallBack ""
}
