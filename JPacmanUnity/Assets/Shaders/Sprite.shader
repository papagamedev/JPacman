// Unlit sprite for Entities Graphics: draws a unit quad mesh (0,0)-(1,1), placed and textured
// per entity through DOTS-instanced properties, so animating a sprite only changes component data.
// Matches the built-in Sprites-Default look (premultiplied alpha blending, no lighting).
Shader "JPacman/Sprite"
{
    Properties
    {
        [MainTexture] _MainTex ("Texture", 2D) = "white" {}
        [MainColor] _Color ("Color", Color) = (1, 1, 1, 1)
        // xy: UV offset, zw: UV size of the sprite in the texture
        _SpriteUV ("Sprite UV Rect", Vector) = (0, 0, 1, 1)
        // xy: quad offset from the pivot, zw: quad size, in world units
        _SpriteQuad ("Sprite Quad Rect", Vector) = (-0.5, -0.5, 1, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
        }

        Blend One OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _SpriteUV;
                float4 _SpriteQuad;
            CBUFFER_END

            #ifdef UNITY_DOTS_INSTANCING_ENABLED
                UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
                    UNITY_DOTS_INSTANCED_PROP(float4, _Color)
                    UNITY_DOTS_INSTANCED_PROP(float4, _SpriteUV)
                    UNITY_DOTS_INSTANCED_PROP(float4, _SpriteQuad)
                UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)

                #define _Color UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _Color)
                #define _SpriteUV UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _SpriteUV)
                #define _SpriteQuad UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _SpriteQuad)
            #endif

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                float3 positionOS = float3(_SpriteQuad.xy + input.positionOS.xy * _SpriteQuad.zw, input.positionOS.z);
                output.positionCS = TransformObjectToHClip(positionOS);
                output.uv = _SpriteUV.xy + input.uv * _SpriteUV.zw;
                output.color = _Color;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.color;
                color.rgb *= color.a;
                return color;
            }
            ENDHLSL
        }
    }
}
