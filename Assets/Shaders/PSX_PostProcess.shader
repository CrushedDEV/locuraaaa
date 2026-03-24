Shader "Custom/PSX_PostProcess"
{
    Properties
    {
        _Resolution ("Resolution (Vertical)", Range(64, 1080)) = 320
        _ColorDepth ("Color Depth", Range(2, 255)) = 32
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZTest Always ZWrite Off Cull Off

        Pass
        {
            Name "PSX_PostProcess"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _Resolution;
            float _ColorDepth;

            // Define point sampling for that crisp pixel art look
            // Point sampling is already defined by URP (sampler_PointClamp)

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;

                // 1. Pixelation
                // We use _ScreenParams to keep correct aspect ratio
                float aspect = _ScreenParams.x / _ScreenParams.y;
                
                // _Resolution will act as the vertical resolution, and horizontal scales accordingly
                float2 res = float2(_Resolution * aspect, _Resolution);
                
                uv = floor(uv * res) / res;

                // 2. Sample the render texture with point filtering
                half4 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, uv);

                // 3. Low Color Bit Depth (Quantisation)
                color.rgb = floor(color.rgb * _ColorDepth) / _ColorDepth;

                return color;
            }
            ENDHLSL
        }
    }
}
