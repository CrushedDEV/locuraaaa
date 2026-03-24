Shader "ScapeRoom/UVHiddenSymbol"
{
    Properties
    {
        _MainTex ("Hidden Texture", 2D) = "white" {}
        [HDR] _Color ("Tint Color", Color) = (1,1,1,1)
        [HDR] _EmissionColor ("Emission Color", Color) = (0,0,0,0)
        _Softness ("Angle Fade Softness", Range(0.001, 1.0)) = 0.5
        _DistanceSoftness ("Distance Fade Softness", Range(0.001, 1.0)) = 0.5
        
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 5 // SrcAlpha
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 10 // OneMinusSrcAlpha
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 4 // LEqual
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        LOD 100

        Blend [_SrcBlend] [_DstBlend]
        ZWrite Off
        ZTest [_ZTest]

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
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _EmissionColor;
            float _Softness;
            float _DistanceSoftness;

            // Global Variables from UVLightController
            float4 _UVLightPosition;
            float4 _UVLightDirection;
            float4 _UVLightParameters; // X: Range, Y: Cos(Angle/2), Z: IsOn

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                col.rgb += _EmissionColor.rgb;

                float isOn = _UVLightParameters.z;
                if (isOn < 0.5) 
                {
                    clip(-1);
                    return fixed4(0,0,0,0);
                }

                float3 toPixel = i.worldPos - _UVLightPosition.xyz;
                float dist = length(toPixel);

                float range = _UVLightParameters.x;
                if (dist > range)
                {
                    clip(-1);
                    return fixed4(0,0,0,0);
                }

                toPixel /= dist;
                
                float angleCos = _UVLightParameters.y;
                float dotProd = dot(toPixel, _UVLightDirection.xyz);
                
                if (dotProd < angleCos)
                {
                    clip(-1);
                    return fixed4(0,0,0,0);
                }

                // Smooth edges transition (fade at the edge of the light cone and max range)
                float intensityDist = 1.0 - (dist / range);
                float intensityAngle = (dotProd - angleCos) / (1.0 - angleCos);
                
                col.a *= smoothstep(0, _DistanceSoftness, intensityDist) * smoothstep(0, _Softness, intensityAngle);
                
                // Discard pixel if alpha is nearly zero to fix URP always rendering issues
                clip(col.a - 0.01);

                return col;
            }
            ENDCG
        }
    }
}
