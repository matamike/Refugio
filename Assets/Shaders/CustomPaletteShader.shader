Shader "CelShader/CustomPaletteShader"
{
    Properties {
        _Colors ("Colors", 2D) = "white" {}
        _Albedo ("Albedo", Range(0,2)) = 1.0
        _Gamma ("Gamma", Range(0.5, 3)) = 1.0
    }
    
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf PaletteCelShading

        sampler2D _Colors;

        // https://docs.unity3d.com/Manual/GPUInstancing.html
        #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(half, _Albedo)
            UNITY_DEFINE_INSTANCED_PROP(half, _Gamma)
        UNITY_INSTANCING_BUFFER_END(Props)
        
        struct Input {
            float2 uv_MainTex;
        };
        void surf (Input IN, inout SurfaceOutput o)
        {
            // o.Albedo = UNITY_ACCESS_INSTANCED_PROP(Props, _Albedo);
            o.Albedo = 1;
            o.Alpha = 1;
            o.Gloss = 0;
            o.Specular = 0;
            o.Emission = 0;
        }

        half4 LightingPaletteCelShading (SurfaceOutput s, half3 lightDir, half atten) {
            half NdotL = dot (s.Normal, lightDir);
            half diff = NdotL * 0.5 + 0.5;
            half4 c;
            c.rgb = s.Albedo * _LightColor0.rgb * (diff * atten);
            c.a = s.Alpha;

            // return c;
            
            half2 map;
            map.x = max(min((2-UNITY_ACCESS_INSTANCED_PROP(Props, _Albedo)) - c.r * UNITY_ACCESS_INSTANCED_PROP(Props, _Gamma) / 2, .99), 0.01);
            map.y = 0;
            return tex2D(_Colors, map);
            half4 t;
            t.r = c.r;
            t.g = 0;
            t.b = tex2D(_Colors, map.x);
            t.a = s.Alpha;
            return t;
        }
      
        ENDCG
    }
    FallBack "Diffuse"
}
